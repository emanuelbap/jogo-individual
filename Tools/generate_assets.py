from PIL import Image, ImageDraw
import random, math, wave, struct
from pathlib import Path
art=Path('Assets/Art'); audio=Path('Assets/Audio')
im=Image.new('RGBA',(64,64)); d=ImageDraw.Draw(im)
d.polygon([(32,2),(41,27),(55,44),(55,53),(37,47),(32,53),(27,47),(9,53),(9,44),(23,27)],fill='#1c87cb',outline='#79e9ff',width=2)
d.polygon([(32,9),(37,34),(32,41),(27,34)],fill='#ccf7ff')
d.polygon([(27,49),(37,49),(35,59),(32,63),(29,59)],fill='#ffb253')
d.rectangle((30,51,33,58),fill='#fff4af'); im.save(art/'ship.png')
im=Image.new('RGBA',(64,64)); d=ImageDraw.Draw(im)
d.polygon([(18,4),(38,3),(54,15),(60,34),(50,52),(32,60),(12,52),(3,34),(7,16)],fill='#a34435',outline='#ff9a53',width=3)
d.ellipse((13,13,29,28),fill='#602c30',outline='#ce6740',width=2)
d.ellipse((34,30,49,46),fill='#602c30',outline='#ce6740',width=2)
d.ellipse((15,39,23,47),fill='#e47a48'); im.save(art/'asteroid.png')
r=random.Random(41); im=Image.new('RGB',(960,540),'#050b1c'); d=ImageDraw.Draw(im)
for i in range(145):
 x,y=r.randrange(960),r.randrange(540); c=r.choice(['#253554','#53658b','#8196b7']); d.rectangle((x,y,x+1,y+1),fill=c)
for x,y in [(142,94),(802,185),(430,390),(690,470)]:
 d.line((x-3,y,x+3,y),fill='#91b8d1');d.line((x,y-3,x,y+3),fill='#91b8d1')
im.save(art/'background.png')
# Sons originais sintetizados: onda simples com envelope para evitar estalos.
sr=22050
def save(name, data):
 with wave.open(str(audio/name),'w') as w:
  w.setnchannels(1);w.setsampwidth(2);w.setframerate(sr)
  w.writeframes(b''.join(struct.pack('<h',int(max(-1,min(1,v))*32767)) for v in data))
def tone(freq,t,duration):
 env=min(1,t/0.008)*min(1,(duration-t)/0.04)
 return math.sin(2*math.pi*freq*t)*max(0,env)
for name, dur, f1, f2 in [('start.wav',.25,440,880),('hit.wav',.22,160,55),('gameover.wav',.7,330,70)]:
 save(name,[.4*tone(f1+(f2-f1)*(i/sr)/dur,i/sr,dur) for i in range(int(sr*dur))])
notes=[220,0,329.63,261.63,220,329.63,392,329.63,174.61,0,261.63,220,196,293.66,329.63,196]
data=[]; step=.25
for i in range(sr*8):
 t=i/sr; n=int(t/step)%len(notes); local=t%step; f=notes[n]
 melody=.13*tone(f,local,step) if f else 0
 bass=.13*tone(110 if int(t/2)%2==0 else 87.31,t%.5,.5)
 beat=.10*math.sin(2*math.pi*(65-25*(t%.5))*t)*math.exp(-35*(t%.5))
 data.append(melody+bass+beat)
save('music.wav',data)
