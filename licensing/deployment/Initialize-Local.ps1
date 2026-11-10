param([string]$Docker = 'C:\Program Files\Docker\Docker\resources\bin\docker.exe')
$ErrorActionPreference = 'Stop'
$licensingRoot = Split-Path $PSScriptRoot -Parent
$localDirectory = Join-Path $licensingRoot '.local'
$configPath = Join-Path $localDirectory 'server.json'
$composePath = Join-Path $PSScriptRoot 'compose.yaml'
if (Test-Path $configPath) { throw 'Local server.json already exists. Reuse it; initialization never resets existing data.' }
New-Item -ItemType Directory -Force $localDirectory | Out-Null
function RandomHex([int]$count) {
    $bytes = New-Object byte[] $count
    $generator = [Security.Cryptography.RandomNumberGenerator]::Create()
    try { $generator.GetBytes($bytes) } finally { $generator.Dispose() }
    ([BitConverter]::ToString($bytes) -replace '-', '').ToLowerInvariant()
}
$utf8 = [Text.UTF8Encoding]::new($false)
$environmentPath = Join-Path $localDirectory 'keygen.env'
if (Test-Path $environmentPath) {
    $values = @{}
    foreach ($line in [IO.File]::ReadAllLines($environmentPath)) {
        if ($line -match '^([^=]+)=(.*)$') { $values[$Matches[1]] = $Matches[2] }
    }
    $account = $values.KEYGEN_ACCOUNT_ID
    $email = $values.KEYGEN_ADMIN_EMAIL
    $password = $values.KEYGEN_ADMIN_PASSWORD
} else {
    $account = [guid]::NewGuid().ToString()
    $email = 'coh3-owner@example.invalid'
    $password = RandomHex 32
    $postgresPassword = RandomHex 32
    $lines = @(
        'KEYGEN_EDITION=CE','KEYGEN_MODE=singleplayer','KEYGEN_HOST=api.keygen.localhost',
        "KEYGEN_ACCOUNT_ID=$account", "KEYGEN_ADMIN_EMAIL=$email", "KEYGEN_ADMIN_PASSWORD=$password",
        ('SECRET_KEY_BASE=' + (RandomHex 64)), ('ENCRYPTION_DETERMINISTIC_KEY=' + (RandomHex 32)),
        ('ENCRYPTION_PRIMARY_KEY=' + (RandomHex 32)), ('ENCRYPTION_KEY_DERIVATION_SALT=' + (RandomHex 32)),
        "DATABASE_URL=postgres://keygen:$postgresPassword@postgres:5432/keygen", 'REDIS_URL=redis://redis:6379/0',
        'NO_SECRETS=1', 'DISABLE_DATABASE_ENVIRONMENT_CHECK=1'
    )
    [IO.File]::WriteAllLines($environmentPath, $lines, $utf8)
    [IO.File]::WriteAllLines((Join-Path $localDirectory 'postgres.env'), @('POSTGRES_USER=keygen', 'POSTGRES_DB=keygen', "POSTGRES_PASSWORD=$postgresPassword"), $utf8)
}
function Compose([string[]]$Arguments) {
    & $Docker compose -f $composePath @Arguments
    if ($LASTEXITCODE -ne 0) { throw ('docker compose failed: ' + ($Arguments -join ' ')) }
}
Compose @('up','-d','postgres','redis')
$setupMarker = Join-Path $localDirectory 'setup.complete'
if (!(Test-Path $setupMarker)) {
    Compose @('--profile','tools','run','--rm','setup')
    [IO.File]::WriteAllText($setupMarker, [DateTimeOffset]::UtcNow.ToString('O'), $utf8)
}
Compose @('up','-d','web','worker')
$baseUrl = "http://127.0.0.1:47830/v1/accounts/$account/"
$headers = @{ 'Host'='api.keygen.localhost'; 'X-Forwarded-Proto'='https'; 'Accept'='application/vnd.api+json' }
$ready = $false
for ($attempt=0; $attempt -lt 120; $attempt++) {
    try { Invoke-RestMethod -Uri 'http://127.0.0.1:47830/v1/health' -Headers $headers -TimeoutSec 2 | Out-Null; $ready=$true; break }
    catch { Start-Sleep -Milliseconds 1000 }
}
if (!$ready) { throw 'Keygen local health did not become ready; inspect only this compose project logs.' }
$basic = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes("${email}:$password"))
$headers.Authorization = "Basic $basic"
$tokenResponse = Invoke-RestMethod -Method Post -Uri ($baseUrl + 'tokens') -Headers $headers -ContentType 'application/vnd.api+json'
$adminApiToken = $tokenResponse.data.attributes.token
if (!$adminApiToken) { throw 'Keygen did not return an administrator API token.' }
$headers.Authorization = "Bearer $adminApiToken"
function Keygen([string]$Method, [string]$Path, $Body) {
    $parameters = @{ Method=$Method; Uri=($baseUrl+$Path); Headers=$headers; ContentType='application/vnd.api+json' }
    if ($null -ne $Body) { $parameters.Body = ConvertTo-Json -InputObject $Body -Depth 12 -Compress }
    Invoke-RestMethod @parameters
}
try {
$accountResponse = Keygen Get '' $null
# Current CE exposes base64-encoded account public keys in resource meta, not attributes.
$encodedPublicKey = $accountResponse.data.meta.keys.rsa2048
if (!$encodedPublicKey) { throw 'Account meta.keys.rsa2048 public key missing.' }
$publicKey = [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($encodedPublicKey))
if ($publicKey -notmatch '-----BEGIN (RSA )?PUBLIC KEY-----') { throw 'Account RSA public key is not PEM.' }
$product = Keygen Post 'products' @{data=@{type='products'; attributes=@{name='CoH3FowTool'}}}
$productId = $product.data.id
function CreatePolicy([string]$Name) {
    Keygen Post 'policies' @{data=@{type='policies'; attributes=@{
        name=$Name; duration=$null; strict=$true; maxMachines=1; protected=$true;
        authenticationStrategy='NONE'; overageStrategy='NO_OVERAGE'; requireFingerprintScope=$true;
        expirationBasis='FROM_CREATION'; expirationStrategy='RESTRICT_ACCESS'; scheme='RSA_2048_PKCS1_SIGN_V2'
    }; relationships=@{product=@{data=@{type='products';id=$productId}}}}}
}
$permanent = CreatePolicy 'CoH3 permanent (no snapshot TTL)'
$timed = CreatePolicy 'CoH3 timed (adapter sets first activation expiry)'
$productToken = Keygen Post ("products/$productId/tokens") @{data=@{type='tokens'}}
$apiToken = $productToken.data.attributes.token
if (!$apiToken) { throw 'Product API token missing.' }
$serverConfig = @{
    ListenUrl='http://127.0.0.1:47831'; DatabasePath=(Join-Path $localDirectory 'redemptions.db');
    AdminToken=(RandomHex 32); KeygenBaseUrl=$baseUrl; KeygenHost='api.keygen.localhost'; KeygenToken=$apiToken;
    PermanentPolicyId=$permanent.data.id; TimedPolicyId=$timed.data.id; VerificationPublicKeyPem=$publicKey
}
[IO.File]::WriteAllText($configPath, (ConvertTo-Json $serverConfig -Depth 5), $utf8)
[IO.File]::WriteAllText((Join-Path $localDirectory 'client-public.json'), (ConvertTo-Json @{
    serviceUri='http://127.0.0.1:47831/'; verificationPublicKeyPem=$publicKey
}), $utf8)
Write-Output 'READY: actual Keygen CE API bootstrapped; loopback 47830, adapter configuration .local/server.json, public trust root .local/client-public.json.'
} finally {
    # Revoke the temporary admin API token even if subsequent provisioning fails.
    Keygen Delete ("tokens/" + $tokenResponse.data.id) $null | Out-Null
}
