# Manual de Utilizador — CoParenting

> Este manual descreve como usar a app do ponto de vista de um pai/mãe utilizador final. É um documento vivo: cada secção é atualizada à medida que a funcionalidade correspondente é implementada (ver [Plano.docx](../Plano.docx) para o roadmap técnico completo). Enquanto uma funcionalidade não estiver disponível, a secção descreve o comportamento planeado e está marcada como tal.

**Legenda de estado:** 🔴 Planeado · 🟡 Em desenvolvimento · 🟢 Disponível

## Estado atual (18 set 2026)

A primeira funcionalidade do MVP está disponível: registo, login e unidade familiar. As restantes (calendário, chat, despesas) ainda não têm interface utilizável.

| Funcionalidade | Estado |
|---|---|
| Registo e unidade familiar | 🟢 Disponível |
| Calendário partilhado | 🔴 Planeado |
| Chat | 🔴 Planeado |
| Despesas partilhadas | 🔴 Planeado |
| Planos pagos | 🔴 Planeado |

## 1. O que é o CoParenting

O CoParenting é uma aplicação web para pais separados organizarem, num único lugar, o calendário de guarda dos filhos, a comunicação entre ambos e o registo de despesas partilhadas — substituindo a mistura habitual de WhatsApp, folhas de Excel e chamadas telefónicas.

## 2. Registo e unidade familiar 🟢

- Cada progenitor cria uma conta com email e password, ou entra diretamente com a conta Google (sem necessidade de confirmar email nesse caso, já que a Google garante essa verificação).
- No registo com email, é enviado um link de confirmação antes de ser possível iniciar sessão.
- O primeiro progenitor cria a "unidade familiar" (escolhendo o seu papel, Pai ou Mãe) e convida o segundo por email — o convite inclui um link de aceitação válido por 7 dias.
- O convidado só consegue aceitar o convite autenticado com o mesmo email para o qual foi enviado; se tentar com outra conta, a app avisa e permite trocar de sessão sem perder o convite.
- Ambos os progenitores passam a ter acesso aos mesmos dados da família; os filhos serão associados à unidade familiar, não a uma conta individual.
- Um utilizador pode pertencer a mais do que uma unidade familiar (ex: um pai com filhos de mães diferentes participa em duas famílias distintas).
- Cada família está limitada a 2 membros.

## 3. Calendário partilhado 🔴

Planeado:
- Vista mensal e semanal com os eventos da família.
- Criação de eventos pontuais (ex: consulta médica) e recorrentes (ex: semana de guarda alternada).
- Notificação por email quando um evento é criado ou alterado pelo outro progenitor.

## 4. Chat 🔴

Planeado:
- Conversa única entre os dois pais, por família.
- Mensagens em tempo real, com histórico permanente — mensagens não podem ser editadas nem apagadas, para servirem de registo fiável em caso de disputa.
- Envio de anexos simples (ex: comprovativos, fotos).

## 5. Despesas partilhadas 🔴

Planeado:
- Registo de uma despesa com descrição, valor, categoria e upload de recibo.
- Cada despesa fica com estado "Pendente" até o outro progenitor confirmar o pagamento.
- Sem divisão automática de valores no MVP — cada família gere manualmente quem deve o quê.

## 6. Planos e preços 🔴

Planeado (ver secção 8 do plano para detalhe):

| Plano | Preço | Inclui |
|---|---|---|
| Grátis | 0€ | Calendário + chat, sem limite de tempo |
| Family | 6-8€/mês | + despesas, documentos, exportação PDF |
| Family + Legal | 12-15€/mês | + relatórios certificados para tribunal |

Os pagamentos só serão ativados depois da fase de Beta (secção 7 do plano) confirmar retenção de utilizadores.

## 7. Perguntas frequentes

*Esta secção será preenchida com as perguntas reais recolhidas nas entrevistas de validação (secção 2 do plano) e no suporte durante o Beta.*

## 8. Suporte

*Canal de suporte a definir antes do lançamento Alpha (secção 7 do plano).*

---

## Histórico de atualizações deste manual

| Data | Alteração |
|---|---|
| 2026-09-18 | Secção 2 (Registo e unidade familiar) passa a 🟢 Disponível: registo/login (email+password e Google), confirmação de email, criação de família e convites. |
| 2026-09-17 | Criação inicial do manual — nenhuma funcionalidade implementada ainda, apenas setup técnico do projeto. |
