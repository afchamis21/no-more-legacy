# &lt;NoMoreLegacy/&gt; - Backend

### ✨ O motor por trás da modernização. ✨

[![Licença: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![versão](https://img.shields.io/badge/version-1.0.0-blue.svg)](https://github.com/USUARIO/REPO)

## 👥 Contribuidores

-   André Chamis
-   Gabriel Amorim

---

Este é o repositório do backend para o projeto `<NoMoreLegacy/>`. Ele serve como o núcleo lógico da aplicação, recebendo os projetos legados, orquestrando uma pipeline de agentes de IA especializados e executando a complexa tarefa de análise, conversão, teste e empacotamento do código para arquiteturas modernas.

---

## 🏛️ Arquitetura dos Agentes

O sistema funciona através de uma pipeline orquestrada de agentes de IA, cada um com uma responsabilidade única e bem definida.

`[ Projeto .zip ] -> [ Orchestrator ] -> [ FileGrouper ] -> [ ContextExtractor ] -> [ FileConverter ] -> [ TestValidationClient ] -> [ CodeMerger ] -> [ ScaffoldFileAgent ] -> [ Código Modernizado ]`

-   **Orchestrator**: O maestro do processo, responsável por gerenciar o fluxo de dados entre os agentes.
-   **FileGrouper**: Analisa o projeto e agrupa os arquivos por funcionalidade para garantir uma migração coesa.
-   **ContextExtractor**: Lê um grupo de arquivos e extrai seu contexto técnico e funcional (endpoints, bibliotecas, etc.).
-   **FileConverter**: O coração do sistema. Converte o código legado para a nova stack (ex: Struts+JSP -> Spring Boot+Angular). Existem implementações específicas para cada framework.
-   **TestValidationClient**: Gera testes unitários e de integração para o código recém-convertido, garantindo a correção funcional.
-   **CodeMergerAgent**: Resolve conflitos de forma inteligente quando um mesmo arquivo é migrado em diferentes grupos, unindo as funcionalidades.
-   **ScaffoldFileAgent**: Cria os arquivos de projeto boilerplate (`pom.xml`, `package.json`, etc.) para empacotar o código convertido em um projeto executável.

---

## ✅ Status do Projeto: Checklist

Esta seção detalha o progresso do design e engenharia de prompts dos agentes.

### O que já foi feito (Engenharia de Prompt):
-   [x] **FileGrouper**: Prompt finalizado, com regras de filtragem e descrição de grupo.
-   [x] **ContextExtractor**: Prompt finalizado, com extração de bibliotecas e definição da stack de destino.
-   [x] **FileConverter (Struts+JSP)**: Prompt finalizado para conversão full-stack.
-   [x] **FileConverter (JSF)**: Prompt finalizado para conversão full-stack.
-   [x] **FileConverter (JAX-RS)**: Prompt finalizado para conversão backend-only.
-   [x] **FileConverter (AngularJS)**: Prompt finalizado para conversão frontend-only.
-   [x] **TestValidationClient**: Prompt finalizado, com regras de posicionamento de arquivos.
-   [x] **CodeMergerAgent**: Prompt finalizado para resolução de conflitos.
-   [x] **ScaffoldFileAgent (Full-Stack)**: Prompt finalizado para projetos Struts/JSF.
-   [x] **ScaffoldFileAgent (Backend-Only)**: Prompt finalizado para projetos JAX-RS.
-   [x] **ScaffoldFileAgent (Frontend-Only)**: Prompt finalizado para projetos AngularJS.

### Próximos Passos (Desenvolvimento):
-   [ ] **Novo Agente: Framework Detector**: Criar um agente inicial que analisa o projeto e determina o framework legado automaticamente, em vez de exigir a seleção do usuário.
-   [ ] **Refatoração dos Agentes**: Criar implementações específicas (extratores, agrupadores) por framework para aumentar ainda mais a precisão do pipeline.

---

## 🛠️ Tecnologias Utilizadas

Este backend foi construído com uma stack moderna da plataforma .NET:

-   **.NET 8** - A mais recente versão do framework .NET.
-   **ASP.NET Core Web API** - Para a construção de uma API RESTful robusta e de alta performance.
-   **Azure.AI.OpenAI SDK** - Para comunicação com os modelos de linguagem da Azure.
-   **Serilog** (sugestão) - Para logging estruturado e de alta performance.

---

## 📦 Como Começar

Para executar este projeto localmente, siga os passos abaixo.

### Pré-requisitos

-   .NET SDK (versão 8.0 ou superior)
-   Um editor de código como Visual Studio ou VS Code.

### Instalação

1.  **Clone o repositório:**
    ```bash
    git clone [https://github.com/SEU_USUARIO/no-more-legacy-backend.git](https://github.com/SEU_USUARIO/no-more-legacy-backend.git)
    cd no-more-legacy-backend
    ```

2.  **Restaure as dependências:**
    ```bash
    dotnet restore
    ```

3.  **Configure as variáveis de ambiente:**
    Renomeie o arquivo `appsettings.Development.example.json` para `appsettings.Development.json`.

    Edite o arquivo com as suas credenciais do Azure OpenAI:
    ```json
    {
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
      },
      "AzureOpenAI": {
        "Endpoint": "SEU_ENDPOINT_AQUI",
        "ApiKey": "SUA_API_KEY_AQUI",
        "DeploymentName": "NOME_DO_SEU_DEPLOYMENT_GPT4"
      }
    }
    ```

4.  **Execute a aplicação:**
    ```bash
    dotnet run
    ```

Por padrão, a API estará disponível em `https://localhost:7123` ou `http://localhost:5288`. Verifique o terminal para as URLs exatas. Você pode acessar a documentação da API (Swagger/OpenAPI) em `/swagger`.

---

## 📖 API Usage

Para iniciar uma migração, envie uma requisição `POST` para o endpoint principal com o arquivo do projeto.

-   **URL**: `/api/migrate`
-   **Método**: `POST`
-   **Body**: `multipart/form-data`
    -   `framework` (string): ex: "Struts"
    -   `file` (file): O arquivo `.zip` do projeto.

---

## 📜 Licença

Distribuído sob a Licença MIT. Veja `LICENSE.txt` para mais informações.