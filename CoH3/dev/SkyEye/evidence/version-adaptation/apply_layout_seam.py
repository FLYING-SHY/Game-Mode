"""Surgical mirrored edit: route image dependencies through maintenance-only candidate seam."""
import pathlib,re
root=pathlib.Path(__file__).resolve().parents[5]
paths=[root/'CoH3/dev/SkyEye/client/CoH3MemoryTool/PlayerInfoReader.cs',root/'CoH3/dev/COH3SimpleTool/CoH3MemoryTool/PlayerInfoReader.cs']
assert paths[0].is_file(),str(root)
texts=[p.read_text(encoding='utf8') for p in paths];assert texts[0]==texts[1],'Engine copies differ before seam edit'
s=texts[0]
assert 'private long BuildRva45(' not in s
# Every literal module RVA, except image range bounds, is a build dependency.
s,n=re.subn(r'image \+ (\d{7,})(L?)',lambda m:m[0] if int(m[1])==268435456 else 'image + BuildRva45('+m[1]+m[2]+')',s)
assert n>=30,n
assert 'image + (vehicle ? 64557040 : 64557304)' in s
s=s.replace('image + (vehicle ? 64557040 : 64557304)','image + BuildRva45(vehicle ? 64557040 : 64557304)')
s=s.replace('Read(image + lookup + num * 128, 1)','Read(image + BuildRva45(lookup) + num * 128, 1)')
s=s.replace('long num = Q(image + slot);','long num = Q(image + BuildRva45(slot));')
s=s.replace('switch (Q(transform) - image)','switch (LegacyTransformRva45(Q(transform) - image))')
anchor='\tpublic PlayerInfoReader() : this(false)'
assert anchor in s
helpers='''\tprivate long BuildRva45(long legacy)
\t{
#if MAINTENANCE_DIAGNOSTICS
\t\tif (maintenanceDiagnostics) return GameLayout52426.Resolve(legacy);
#endif
\t\treturn legacy;
\t}

\tprivate long LegacyTransformRva45(long actual)
\t{
#if MAINTENANCE_DIAGNOSTICS
\t\tif (maintenanceDiagnostics) return GameLayout52426.LegacyTransform(actual);
#endif
\t\treturn actual;
\t}

'''
s=s.replace(anchor,helpers+anchor)
for proc in ['process','this.process']:
    needle='\t\t\tGameVersion45.RequireVerified(BuildStatus, maintenanceDiagnostics);'
# Two constructors; license check and handle acquisition remain after candidate identity check.
assert s.count(needle)==2
s=s.replace(needle,needle+'\n#if MAINTENANCE_DIAGNOSTICS\n\t\t\tif (maintenanceDiagnostics) GameLayout52426.RequireCandidate(process.MainModule.FileName);\n#endif')
for path in paths:path.write_text(s,encoding='utf8',newline='')
layout=paths[0].with_name('GameLayout52426.cs');target=paths[1].with_name(layout.name)
assert not target.exists();target.write_text(layout.read_text(encoding='utf8'),encoding='utf8',newline='')
print({'readerCopies':len(paths),'literalMatches':n,'candidateLayoutInRelease':False})
