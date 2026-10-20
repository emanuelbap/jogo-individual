"""Monta a capa vetorial usando os próprios sprites do jogo."""
import base64
from pathlib import Path

def image(name):
    return 'data:image/png;base64,' + base64.b64encode(Path('Assets/Art/'+name).read_bytes()).decode()
svg = f'''<svg xmlns="http://www.w3.org/2000/svg" width="630" height="500" viewBox="0 0 630 500">
<rect width="630" height="500" fill="#050b1c"/>
<image href="{image('background.png')}" width="890" height="500" x="-130"/>
<rect x="28" y="28" width="574" height="444" rx="8" fill="none" stroke="#234a65"/>
<text x="315" y="82" text-anchor="middle" font-family="sans-serif" font-size="16" letter-spacing="5" fill="#70d9ff">ARCADE · SOBREVIVÊNCIA</text>
<text x="315" y="155" text-anchor="middle" font-family="sans-serif" font-weight="bold" font-size="55" fill="#e6f8ff">DEFESA</text>
<text x="315" y="214" text-anchor="middle" font-family="sans-serif" font-weight="bold" font-size="55" fill="#65dcff">ESPACIAL</text>
<image href="{image('asteroid.png')}" x="95" y="260" width="65" height="65" style="image-rendering:pixelated"/>
<image href="{image('asteroid.png')}" x="465" y="235" width="82" height="82" style="image-rendering:pixelated"/>
<image href="{image('asteroid.png')}" x="383" y="320" width="45" height="45" style="image-rendering:pixelated"/>
<image href="{image('ship.png')}" x="265" y="285" width="100" height="100" style="image-rendering:pixelated"/>
<text x="315" y="424" text-anchor="middle" font-family="sans-serif" font-size="20" fill="#d7e6fa">TRÊS VIDAS. QUANTO TEMPO VOCÊ AGUENTA?</text>
<text x="315" y="454" text-anchor="middle" font-family="sans-serif" font-size="14" fill="#829ab9">Um jogo de Emanuel Apolinário</text>
</svg>'''
Path('Delivery/cover.svg').write_text(svg)
