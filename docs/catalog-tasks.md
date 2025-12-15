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