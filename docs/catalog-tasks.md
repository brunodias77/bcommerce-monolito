Com base na análise do código-fonte fornecido (`Category.cs` e `CategoryConfiguration.cs`) e na estrutura do projeto (Clean Architecture com CQRS), segue a documentação técnica detalhada para o comando `CreateCategoryCommand`.

---

#CMD-06: CreateCategory (CreateCategoryCommand)**Descrição**
Este comando é responsável por criar uma nova categoria no catálogo de produtos. Ele suporta a criação de categorias raiz (sem pai) ou subcategorias (com pai), gerenciando automaticamente a hierarquia, profundidade (`Depth`) e caminho materializado (`Path`) para navegação eficiente. O comando orquestra a validação de unicidade de *slug* e garante a integridade da árvore de categorias.

##Request (Input)A requisição deve ser enviada para o endpoint de criação de categorias (ex: `POST /api/v1/categories`).

###Estrutura de Dados| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `name` | `string` | **Sim** | Nome de exibição da categoria. Usado para gerar o *slug*. Máximo de 100 caracteres. |
| `parentId` | `Guid?` | Não | Identificador único da categoria pai. Se nulo, a categoria será criada na raiz (Depth 0). |
| `description` | `string?` | Não | Descrição detalhada da categoria para fins de exibição ou SEO. |

###Exemplo de JSON (Request)```json
{
  "name": "Smartphones",
  "parentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "description": "Dispositivos móveis de última geração e acessórios."
}

```

##Regras de Negócio (Business Rules)* **RN-01 (Obrigatório):** O campo `name` não pode ser vazio ou nulo.
* **RN-02 (Unicidade de Slug):** O sistema deve gerar um *slug* amigável (URL-friendly) a partir do `name`. Não é permitida a existência de duas categorias com o mesmo *slug* no banco de dados.
* **RN-03 (Integridade Hierárquica):** Se um `parentId` for informado, a categoria pai deve existir no banco de dados e estar ativa.
* **RN-04 (Limite de Profundidade):** A profundidade máxima da hierarquia de categorias é de 5 níveis (0 a 4). Tentativas de criar subcategorias além desse limite devem ser bloqueadas (restrição imposta pela Entidade de Domínio).
* **RN-05 (Caminho Materializado):** O campo `Path` deve ser construído concatenando o `Path` do pai com o `Id` da nova categoria (ex: `paiId/novoId`), ou apenas o `novoId` se for raiz.

##Fluxo de Processamento (Workflow)1. **Validação de Contrato:** O `CreateCategoryCommandValidator` verifica se o `name` foi preenchido e respeita o tamanho máximo.
2. **Geração de Slug:** O *Handler* gera o *slug* preliminar baseado no nome fornecido (caixa baixa, remoção de acentos, substituição de espaços por hifens).
3. **Verificação de Duplicidade:** O repositório (`ICategoryRepository`) é consultado para verificar se já existe uma categoria com o *slug* gerado.
* *Exceção:* Se existir, retorna erro de conflito ou validação (`Category with this name/slug already exists`).


4. **Verificação do Pai (Condicional):**
* Se `parentId` != null:
* O repositório busca a entidade Pai.
* *Exceção:* Se não encontrado, lança `EntityNotFoundException`.
* A entidade `Category` valida se `Parent.Depth` < 5.




5. **Instanciação do Domínio:** O método estático factory `Category.Create(name, description, parent)` é invocado.
* Define `IsActive = true`.
* Define `SortOrder = 0`.
* Calcula `Depth` e `Path`.


6. **Persistência:** A nova entidade é adicionada ao repositório.
7. **Commit:** O `IUnitOfWork.CommitAsync` é chamado para salvar as alterações no banco de dados.
8. **Retorno:** O ID da nova categoria é retornado encapsulado em um objeto `Result`.

##Response (Output)###Sucesso (201 Created)Retorna o ID da categoria criada.

```json
{
  "categoryId": "d290f1ee-6c54-4b01-90e6-d701748f0851"
}

```

###Erro (400 Bad Request / 409 Conflict)Exemplo de resposta padrão para violação de regras de negócio (ex: profundidade excedida ou nome duplicado).

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Business Validation Error",
  "status": 400,
  "detail": "Maximum category depth (5) exceeded.",
  "errors": {
    "ParentId": [
      "Cannot add subcategory to a category at maximum depth."
    ]
  }
}

```

*********************************************************************************************************************************************************

Com base na análise do código-fonte fornecido (`Product.cs`, `ProductConfiguration.cs`, `ProductStatus.cs`) e nos requisitos do sistema, segue a documentação técnica detalhada para o comando `CreateProductCommand`.

---

#CMD-01: CreateProduct (CreateProductCommand)**Descrição**
Este comando é responsável pela criação de um novo produto no catálogo. Ele atua como o ponto de entrada para a instancialização do *Aggregate Root* `Product`. O comando garante que o produto seja criado com o status inicial de `Draft` (Rascunho), assegura a unicidade de identificadores comerciais (SKU) e de navegação (Slug), e dispara eventos de domínio para integração com outros contextos (como indexação em mecanismos de busca).

##Request (Input)A requisição deve ser enviada para o endpoint de produtos (ex: `POST /api/v1/products`).

###Estrutura de Dados| Campo | Tipo | Obrigatório | Descrição |
| --- | --- | --- | --- |
| `name` | `string` | **Sim** | Nome comercial do produto. Utilizado para gerar o *slug*. Máximo de 150 caracteres. |
| `sku` | `string` | **Sim** | *Stock Keeping Unit*. Código único identificador do produto. Máximo de 100 caracteres. |
| `price` | `decimal` | **Sim** | Preço de venda base do produto. Deve ser maior ou igual a zero. |
| `description` | `string` | Não | Descrição detalhada do produto (HTML ou Texto). |
| `initialStock` | `int` | Não | Quantidade inicial em estoque. Padrão é 0. Não pode ser negativo. |
| `categoryId` | `Guid?` | Não | ID da categoria à qual o produto pertence. |
| `brandId` | `Guid?` | Não | ID da marca do produto. |

###Exemplo de JSON (Request)```json
{
  "name": "Smartphone XYZ Pro 128GB",
  "sku": "SP-XYZ-128-BLK",
  "price": 2499.90,
  "description": "O mais novo smartphone da linha XYZ com processador de última geração.",
  "initialStock": 50,
  "categoryId": "d290f1ee-6c54-4b01-90e6-d701748f0851",
  "brandId": "a1b2c3d4-e5f6-7890-1234-567890abcdef"
}

```

##Regras de Negócio (Business Rules)* **RN-01 (Identidade Única de SKU):** O código SKU fornecido deve ser único em todo o sistema. A tentativa de criar um produto com SKU existente deve resultar em erro de conflito.
* **RN-02 (Geração e Unicidade de Slug):** O sistema deve gerar um *slug* (URL-friendly) a partir do `name`. Este *slug* deve ser único. Caso o *slug* gerado já exista, o sistema deve tratar a colisão (retornando erro ou sufixando, conforme política definida, neste caso assume-se erro de validação).
* **RN-03 (Valores Monetários):** O `price` não pode ser negativo.
* **RN-04 (Estoque Inicial):** O `initialStock` não pode ser negativo.
* **RN-05 (Status Inicial):** Todo novo produto deve ser criado obrigatoriamente com o status `Draft` (Rascunho), garantindo que não apareça na vitrine até ser explicitamente publicado.
* **RN-06 (Integridade Relacional):** Caso `categoryId` ou `brandId` sejam informados, as respectivas entidades devem existir no banco de dados.

##Fluxo de Processamento (Workflow)1. **Validação de Contrato (Fail-Fast):** O `CreateProductCommandValidator` verifica a presença dos campos obrigatórios (`name`, `sku`, `price`) e limites de caracteres.
2. **Verificação de Unicidade:**
* O *Handler* consulta o `IProductRepository` para verificar se já existe um produto com o `sku` informado.
* *Exceção:* Retorna erro se o SKU já estiver em uso.


3. **Geração e Verificação de Slug:**
* O *Handler* simula a geração do *slug* (ou delega para a entidade posteriormente, mas verifica a pré-existência).
* Consulta o repositório para garantir que o *slug* derivado do nome é único.


4. **Instanciação do Domínio:** O método estático factory `Product.Create(...)` é invocado.
* Define `Status = ProductStatus.Draft` (0).
* Define datas de criação (`CreatedAt`).
* Aplica regras de negócio internas (RN-03 e RN-04).
* Gera o Evento de Domínio: `ProductCreatedEvent`.


5. **Persistência:** A nova entidade é adicionada ao `IProductRepository`.
6. **Commit:** O `IUnitOfWork.CommitAsync` persiste os dados transacionalmente.
7. **Pós-Processamento (Eventos):**
* O `ProductCreatedEvent` é despachado (via `MediatR` ou *Message Broker*).
* Listeners reagem ao evento para indexar o novo produto no **ElasticSearch** (para busca textual) e/ou atualizar réplicas de leitura.


8. **Retorno:** O ID do novo produto é retornado.

##Response (Output)###Sucesso (201 Created)Retorna o identificador único do produto criado.

```json
{
  "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}

```

###Erro (400 Bad Request / 409 Conflict)Exemplo de erro de validação de domínio (ex: preço negativo) ou conflito de SKU.

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Business Validation Error",
  "status": 400,
  "detail": "Price cannot be negative.",
  "errors": {
    "Price": [
      "The value '-10.00' is not valid for Price."
    ]
  }
}

```

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Conflict Error",
  "status": 409,
  "detail": "Product with SKU 'SP-XYZ-128-BLK' already exists."
}

```


*********************************************************************************************************************************************************