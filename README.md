# Atom Gauntlet: Protocol

Beat 'em up de plataforma em 3D, feito em Unity. Polo, um ladrão preso em Carcere,
uma estação prisional espacial, rouba uma Atom Gauntlet, uma luva capaz de
reorganizar átomos e criar armas, e tenta escapar enfrentando guardas,
prisioneiros e o chefe do crime Orion.

▶️ **Gameplay:** https://youtu.be/Ae4IK1Qoruo
⬇️ **Download (Windows):** veja em [Releases](../../releases)

Projeto Integrado do 1º ano de Design de Jogos Digitais (IPCA), feito por uma
equipe de quatro pessoas. Revisado em 2026 por Wilk Lacerda: correção de bugs,
retrabalho do combate e balanceamento completo.

## Como jogar

| Ação | Tecla |
|---|---|
| Mover | WASD |
| Pular | Espaço |
| Atacar | Clique do Mouse |
| Espada / Martelo / Machado | 1 / 2 / 3 |
| Interagir / Executar (segurar) | E |
| Pausa | Esc |

A luva gasta energia a cada golpe e só recarrega nos checkpoints. Sem energia,
Polo luta de punhos. Inimigos com pouca vida podem ser executados com o machado,
o que economiza energia.

## Equipe

| Nome | Contribuição |
|---|---|
| Wilk Lacerda | Maior parte da programação, Orion (boss final) e suas animações, fase final (Hangar), apoio no level design e na história |
| João Victor Ribeiro | Polo (protagonista), fase 1 (Celas) |
| Maryana Pinheiro | Carcereiros, fase 2 (Manutenção) |
| Rafael Alexandre | Prisioneiros, fase 3 (Armaria) |

## O que mudou na revisão de 2026

**Combate do jogador**
- O dano era aplicado no clique, separado da animação. Cada ataque passou a ter
  preparação, impacto e recuperação, com o dano no quadro em que a arma acerta.
- Cada arma tem duração própria (a animação é acelerada ou desacelerada para
  caber), buffer de input para emendar golpes, e hitstop no impacto.
- A execução com machado tinha parado de funcionar: um componente duplicado no
  Canvas fazia o jogador mandar o ícone para o objeto errado, sem erro nenhum.

**Inimigos com golpes legíveis**
- Guarda: o dano saía mais de um segundo antes da espada descer. Agora o golpe
  tem aviso, a direção trava antes do impacto e só acerta à frente.
- Atirador: tiro sincronizado com a animação, cuspe em arco que dá para pular,
  e correção de um bug em que um tiro "cancelado" saía mesmo assim.
- Bumerangue: perseguição só no início do voo e uma janela para revidar depois
  que ele pega o bumerangue de volta.
- Boxeador: combo de dois socos com uma única animação, com velocidades
  diferentes por trecho para deixar o segundo soco anunciado.

**Economia de energia**
- Custos recalculados contra a quantidade de inimigos de cada trecho entre
  checkpoints. O limiar de execução subiu para 40%, e o confronto final da
  Armaria só é vencível executando.

**Orion**
- O relógio dos ataques especiais continuava correndo durante os ataques, então
  não sobrava tempo para revidar. Agora existe uma pausa real, que encolhe
  conforme o jogador escolhe bater em vez de selar.
- O selo estava fora do alcance do jogador e sem aviso visual. O ataque em
  todas as direções ganhou projéteis menores e baixos, que dá para pular.

**Câmera e build**
- Tremor de impacto sem desvio de posição, câmera limitada à arena do Orion.
- Build de Windows em Direct3D 11, para evitar um crash intermitente na
  primeira execução.

## Técnico

- Unity 6 (6000.6.0f1), C#
- Scripts em `Assets/Game/Scripts`: `Player`, `Enemies` (e `Enemies/Orion`),
  `Systems`, `Core`, `UI`, `Camera`
- O projeto original usava Unity Version Control. Este repositório começa no
  estado da entrega do 1º ano, e cada correção da revisão é um commit separado.