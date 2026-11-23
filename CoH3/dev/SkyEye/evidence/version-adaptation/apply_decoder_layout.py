from pathlib import Path
root=Path(r'F:\WorkSpace\Game\CoH3\dev')
for directory in [root/'SkyEye/client/CoH3MemoryTool',root/'COH3SimpleTool/CoH3MemoryTool']:
 def patch(name, pairs):
  p=directory/name;s=p.read_text(encoding='utf-8-sig')
  for old,new in pairs:
   assert s.count(old)==1,(name,old,s.count(old))
   s=s.replace(old,new)
  p.write_text(s,encoding='utf8')
 patch('GameLayout52426.cs', [('\t\t{ 65307976, 0x3E49618 },','\t\t{ 65313128, 0x3E4AA38 }, // ProjectileDynamics / static RTTI candidate; event fields pending\n\t\t{ 65307976, 0x3E49618 },')])
 patch('ProjectileLogic.cs', [('bool playerSource = false)','bool playerSource = false, Func<long, long> resolveRva = null)'),('image + 65313128','image + (resolveRva?.Invoke(65313128) ?? 65313128)'),('image + 65307976','image + (resolveRva?.Invoke(65307976) ?? 65307976)')])
 patch('AirLogic.cs', [('uint id, uint index)\n\t{\n\t\tif (body == null','uint id, uint index, Func<long, long> resolveRva = null)\n\t{\n\t\tif (body == null'),('bool point = false)','bool point = false, Func<long, long> resolveRva = null)'),('uint id, uint index)\n\t{\n\t\tif (body.Length','uint id, uint index, Func<long, long> resolveRva = null)\n\t{\n\t\tif (body.Length'),('image + 64559016','image + (resolveRva?.Invoke(64559016) ?? 64559016)'),('image + 64568064','image + (resolveRva?.Invoke(64568064) ?? 64568064)'),('image + 65313128','image + (resolveRva?.Invoke(65313128) ?? 65313128)'),('image + 65307976','image + (resolveRva?.Invoke(65307976) ?? 65307976)')])
 patch('SupportCatalogue49.cs', [('long image, long identity)','long image, long identity, Func<long, long> resolveRva = null)'),('image + 64568064','image + (resolveRva?.Invoke(64568064) ?? 64568064)')])
 patch('PlayerInfoReader.cs', [('out var dz, airDefinition.Point)','out var dz, airDefinition.Point, BuildRva45)'),('array5, ext, image, arena, value4.Entity, num14, num12)','array5, ext, image, arena, value4.Entity, num14, num12, BuildRva45)'),('array6, image, arena, value4.Entity, num14, num12)','array6, image, arena, value4.Entity, num14, num12, BuildRva45)'),('text3 != null, flag) &&','text3 != null, flag, BuildRva45) &&'),('SupportCatalogue49.Active(array, image, row.PlayerIdentity)','SupportCatalogue49.Active(array, image, row.PlayerIdentity, BuildRva45)')])
print('Mirrored decoder layout seam patched; no default fixture behavior changed.')
