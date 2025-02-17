# Resumo da prova
## O que foi realizado
- Um simulador de elétrica que (atualmente) funciona com geradores, interruptores, diodos (LED ou não) e resistores (luminosos ou não);
- - Cada componente é um ElectricComponent, para criar novos componentes basta criar uma classe que herda ElectricComponent e implementar os métodos;
- Resistores definem suas cores automaticamente a partir da sua resistência;
- Associação de resistência (série/paralelo) funcional (misturas complexas de série e paralelo foram proramadas, mas não foram testadas, podem ou não funcionar corretamente);
- Associação de geradores limitada a geradores em série de mesma polaridade;
- Callbacks para definir o que fazer quando um componente está em curto circuito;
- Modelo de circuito a partir de princípios físicos (como lei de Ohm e associação de resistores);
- Ao clicar em algumas coisas (como o abajur e a fonte), é dado um zoom para melhor visualização
- Testado em Android e WebGl, funcionam como esperado.
## Tecnologias utilizadas
- Unity 6000.0.36f1
- C#
- Blender
- WebGL
- Android
## O que não foi possível fazer
- Porte para VR: não tenho um headset XR nem experiência com XR controllers, então não saberia quais inputs definir.
- Associação de geradores em paralelo e com polaridade invertida: apesar de ter o conhecimento necessário, mapear geradores em paralelo e com polaridade invertida seria uma complicação adicional maior do que o tempo disponível para a prova.
## Observações adicionais
- Todos os valores estão com base no SI, ou seja, potência em Watts, corrente em Ampères, tensão em Volts e resistência em Ohms;
- Resistores com valores de resistência e tolerância fora da tabela de cores convencional vão ser arredondados (resistência para baixo e tolerância para cima), ou seja, um resistor com valor de 1k315 Ω e tolerância 3% será mostrado como marrom, laranja, marrom (se for resistor de precisão), vermelho (convencional)/marrom (precisão) e dourado;
- Na prática, no simulador o valor de tolerância não muda em nada no valor do resistor, mas foi mantido para definir a cor da última linha.