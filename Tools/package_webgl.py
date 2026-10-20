"""Empacota o build com index.html na raiz, como o itch.io exige."""
from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED

project = Path(__file__).resolve().parent.parent
build = project / 'Delivery' / 'WebGL'
output = project / 'Delivery' / 'DefesaEspacial-WebGL.zip'
if not (build / 'index.html').is_file():
    raise SystemExit('Gere primeiro o build WebGL pelo Unity.')
with ZipFile(output, 'w', ZIP_DEFLATED) as archive:
    for file in sorted(build.rglob('*')):
        if file.is_file():
            archive.write(file, file.relative_to(build))
with ZipFile(output) as archive:
    assert 'index.html' in archive.namelist()
    assert archive.testzip() is None
print(f'ZIP pronto: {output} ({output.stat().st_size / 1024 / 1024:.1f} MB)')
