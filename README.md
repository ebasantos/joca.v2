# WhatsApp Chatbot para Imobiliária

Sistema de chatbot para WhatsApp que automatiza o contato com potenciais clientes de imóveis, utilizando processamento de linguagem natural para identificar interessados.

## Funcionalidades

- Upload de planilhas Excel com contatos
- Chatbot automático via WhatsApp
- Dashboard com estatísticas de atendimento
- Notificação automática para equipe de atendimento
- Interface moderna e responsiva

## Requisitos

- Node.js 14+
- MongoDB
- NPM ou Yarn
- WhatsApp Web

## Instalação

1. Clone o repositório:
```bash
git clone [url-do-repositorio]
cd whatsapp-chatbot
```

2. Instale as dependências do backend:
```bash
cd backend
npm install
```

3. Instale as dependências do frontend:
```bash
cd ../frontend
npm install
```

4. Configure as variáveis de ambiente:
Crie um arquivo `.env` na pasta `backend` com as seguintes variáveis:
```
MONGODB_URI=sua_url_do_mongodb
PORT=5000
```

## Executando o Projeto

1. Inicie o backend:
```bash
cd backend
npm run dev
```

2. Inicie o frontend:
```bash
cd frontend
npm start
```

3. Acesse a aplicação em `http://localhost:3000`

## Uso

1. Faça upload de uma planilha Excel contendo os números de telefone dos contatos
2. O sistema iniciará automaticamente as conversas via WhatsApp
3. Acompanhe o progresso no dashboard
4. Os contatos interessados serão automaticamente encaminhados para a equipe de atendimento

## Estrutura da Planilha Excel

A planilha deve conter as seguintes colunas:
- phone/telefone: Número do telefone do contato
- name/nome (opcional): Nome do contato

## Tecnologias Utilizadas

- Frontend: React, Material-UI, Recharts
- Backend: Node.js, Express
- Banco de Dados: MongoDB
- WhatsApp: whatsapp-web.js
- Processamento de Dados: xlsx

## Contribuição

1. Faça um fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## Licença

Este projeto está sob a licença MIT. Veja o arquivo `LICENSE` para mais detalhes. 