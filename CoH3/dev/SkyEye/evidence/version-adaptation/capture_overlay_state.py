"""Capture only explicitly selected maintenance PID windows; no screen-wide capture."""
import argparse,runpy,sys,pathlib,json,datetime
p=argparse.ArgumentParser();p.add_argument('--pid',type=int,required=True);p.add_argument('--tag',required=True);a=p.parse_args()
if not a.tag.replace('-','').isalnum(): raise ValueError('Bad tag')
root=pathlib.Path(__file__).resolve().parent
sys.argv=['window_probe.py','--pid',str(a.pid)]
d=runpy.run_path(str(root.parent/'adaptation-52426-20261121-004302'/'window_probe.py'))
rows=[]
for hwnd,title,r in d['windows']:
 out=root/(a.tag+'-'+str(hwnd)+'.png')
 d['capture_window'](hwnd,r[2]-r[0],r[3]-r[1]).save(out)
 rows.append({'hwnd':hwnd,'isControlPanel':bool(title),'rect':r,'image':str(out)})
report={'utc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'pid':a.pid,'tag':a.tag,'windows':rows}
(root/(a.tag+'.json')).write_text(json.dumps(report,indent=2),encoding='utf8')
print(json.dumps(report))
