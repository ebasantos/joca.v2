from openpyxl import Workbook
from datetime import datetime

# Criar um novo workbook
wb = Workbook()
ws = wb.active

# Definir os cabeçalhos
headers = ['Nome', 'Telefone', 'Status', 'Data de Importação']
ws.append(headers)

# Dados de exemplo
contatos = [
    ['João Silva', '5565984193431', 'Pendente', datetime.now().strftime('%d/%m/%Y')],
    ['Maria Santos', '5565984193431', 'Pendente', datetime.now().strftime('%d/%m/%Y')],
    ['Pedro Oliveira', '5565984193431', 'Pendente', datetime.now().strftime('%d/%m/%Y')],
    ['Ana Costa', '5565984193431', 'Pendente', datetime.now().strftime('%d/%m/%Y')],
    ['Carlos Souza', '5565984193431', 'Pendente', datetime.now().strftime('%d/%m/%Y')]
]

# Adicionar os dados
for contato in contatos:
    ws.append(contato)

# Salvar o arquivo
wb.save('contatos.xlsx')
print("Planilha criada com sucesso!") 