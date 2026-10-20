# Defesa Espacial

**Jogue no navegador:** https://emanuel-apolinario.itch.io/defesa-espacial

**Inscrição na jam:** https://itch.io/jam/projeto-intro-unity-insper-20262/rate/5197267

Jogo 2D de sobrevivência feito em **Unity 6000.6.6f1** para o Projeto Introdução ao Unity do Insper (2026.2).

Controle uma nave, desvie dos asteroides e das naves inimigas e tente sobreviver o máximo possível. A pontuação é independente do tempo: cada asteroide que passa sem atingir a nave vale **10 pontos** e cada nave inimiga desviada vale **25 pontos**. A nave começa com três vidas; os asteroides ficam mais rápidos e frequentes com o passar do tempo. Após uma colisão, a nave pisca e fica protegida por 1,5 segundo.

## Como jogar

| Ação | Teclado | Controle Xbox |
| --- | --- | --- |
| Mover em todas as direções | W A S D ou setas | Analógico esquerdo ou direcional |
| Iniciar / confirmar botão | Enter; Espaço inicia a partida | A |
| Pausar / continuar | Esc | Start |
| Navegar nos menus | Setas e Enter, ou mouse | Direcional e A |

O menu permite iniciar, controlar o som e consultar os recordes de pontos e de tempo. Ao perder todas as vidas, os pontos e o tempo finais aparecem e é possível tentar novamente ou voltar ao menu. Trocar de aba pausa a partida. Os recordes e a preferência de áudio são salvos localmente no navegador, podendo ser apagados ao limpar os dados do site.

## Abrir no Unity

1. Abra esta pasta pelo Unity Hub com Unity **6000.6.6f1**.
2. Abra `Assets/Scenes/DefesaEspacial.unity`, use **Defesa Espacial → 3 - Tela 600 x 480** e pressione **Play**.
3. Se precisar reconstruir a cena, use **Defesa Espacial → 1 - Montar cena**. Essa opção recria a cena; salve suas alterações antes.

A cena do jogo é `DefesaEspacial` e é a única incluída no build.

## Gerar e testar WebGL

Instale o módulo **WebGL Build Support** pelo Unity Hub. No editor, use **Defesa Espacial → 2 - Gerar WebGL**. O resultado fica em `Delivery/WebGL/`. No Linux, a conversão de áudio do Unity também requer `ffmpeg` no PATH.

Também é possível gerar pelo terminal, com o editor do projeto fechado:

```bash
BEE_BUILD_THREADS=2 /caminho/para/Unity -batchmode -nographics -quit -buildTarget WebGL -projectPath "$PWD" -executeMethod SpaceProjectBuilder.BuildWebGL -logFile /tmp/defesa-build.log
```

Para testar, sirva o build por HTTP (abrir `index.html` diretamente não basta):

```bash
python3 -m http.server 8000 --directory Delivery/WebGL
```

Acesse `http://localhost:8000`. A compressão WebGL foi desativada para simplificar o upload no itch.io. Execute `python3 Tools/package_webgl.py` para gerar `Delivery/DefesaEspacial-WebGL.zip`, ou faça um ZIP do **conteúdo** de `Delivery/WebGL`, com `index.html` na raiz do ZIP, e selecione **HTML — jogar no navegador** no itch.io. Use o tamanho **600 × 480** e habilite tela cheia.

## Organização dos scripts

- `GameManager.cs`: menu, partida, pausa, vidas, cronômetro, pontos e recordes.
- `PlayerController.cs`: movimento em todas as direções, limites e proteção após colisão.
- `AsteroidSpawner.cs`: criação dos asteroides, limite de inimigos e dificuldade ao longo do tempo.
- `EnemyShip.cs`: perseguição lateral durante a aproximação e ataque em linha reta.
- `Asteroid.cs`: queda, rotação e remoção fora da tela.
- `GameUI.cs`: textos, botões, placar e menus usando Unity UI.
- `GameAudio.cs`: música, efeitos e preferência de som.
- `CameraFrame.cs`: enquadramento da arena em 600 × 480.
- `Editor/SpaceProjectBuilder.cs`: montagem da cena e geração do build.

Parâmetros como velocidade, intervalo dos asteroides e duração da proteção podem ser alterados no Inspector. A dificuldade tem limites de velocidade e frequência para manter a partida possível. A dificuldade cresce até os 40 segundos e depois para de aumentar: os asteroides chegam a 6 unidades/s, com intervalo mínimo de 0,65 s. A primeira nave inimiga chega aos 8 segundos; existem no máximo duas ao mesmo tempo. Elas perseguem lateralmente e seguem reto na aproximação final, sempre mais lentas que o jogador. Não há tiros, upgrades ou fases.

## Créditos e referências

- Responsável pelo projeto: **Emanuel Apolinário**.
- Sprites `ship.png`, `asteroid.png`, `enemy.png` e `background.png`: criados especificamente para este projeto por desenho procedural, sem imagens de terceiros.
- `Tools/generate_assets.py` gera novamente as imagens e os sons (Python 3 + Pillow).
- Engine, Unity UI e Input System: Unity Technologies. Fonte: `LegacyRuntime.ttf`, fornecida pelo Unity.
