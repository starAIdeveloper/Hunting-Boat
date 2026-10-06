"""Static integrity check, explicitly not Unity compilation or gameplay QA."""
import json,re,uuid
from pathlib import Path
from tree_sitter import Language,Parser
import tree_sitter_c_sharp
ROOT=Path(__file__).resolve().parents[1]
parser=Parser(Language(tree_sitter_c_sharp.language()))
errors=[];files=list((ROOT/'Assets').rglob('*.cs'))
for path in files:
    tree=parser.parse(path.read_bytes())
    if tree.root_node.has_error:errors.append('C# parse error: '+str(path.relative_to(ROOT)))
guids={}
for meta in (ROOT/'Assets').rglob('*.meta'):
    match=re.search(r'^guid: ([a-f0-9]{32})$',meta.read_text(),re.M)
    if not match:errors.append('Missing GUID: '+str(meta))
    elif match[1] in guids:errors.append('Duplicate GUID: '+match[1])
    else:guids[match[1]]=meta
for path in (ROOT/'Assets').rglob('*'):
    if path.suffix=='.meta':continue
    if not Path(str(path)+'.meta').exists():errors.append('Missing meta: '+str(path.relative_to(ROOT)))
for path in list((ROOT/'Assets').rglob('*.unity'))+list((ROOT/'ProjectSettings').glob('EditorBuildSettings.asset')):
    for guid in re.findall(r'guid: ([a-f0-9]{32})',path.read_text()):
        if guid not in guids:errors.append('Unresolved scene GUID: '+guid)
for path in list((ROOT/'Assets').rglob('*.asmdef'))+[ROOT/'Packages/manifest.json']:
    json.loads(path.read_text())
shader_names=set(re.findall(r'^Shader "([^"]+)"', '\n'.join(p.read_text() for p in (ROOT/'Assets').rglob('*.shader')),re.M))
for path in files:
    for name in re.findall(r'Shader.Find\("([^"]+)"\)',path.read_text()):
        if name.startswith('HuntingBoat/') and name not in shader_names:errors.append('Unresolved custom shader: '+name)
for path in (ROOT/'Assets').rglob('*.shader'):
    s=path.read_text()
    if s.count('{')!=s.count('}') or s.count('CGPROGRAM')!=s.count('ENDCG'):errors.append('Shader block mismatch: '+str(path))
report={'static_status':'passed' if not errors else 'failed','csharp_files_parsed':len(files),'asset_guids':len(guids),'custom_shaders':sorted(shader_names),'checks':['C# syntax parsing','unique asset GUIDs','all asset metadata present','scene script/build GUID resolution','assembly/package JSON','custom shader lookup resolution','shader block balance'],'unity_editor_available':False,'unity_compile':'not run','unity_editmode_tests':'not run','unity_playmode':'not run','native_build':'not run','errors':errors}
(ROOT/'artifacts').mkdir(exist_ok=True);(ROOT/'artifacts/source-validation.json').write_text(json.dumps(report,indent=2));print(json.dumps(report,indent=2))
if errors:raise SystemExit(1)
