namespace BuildingBlocks.Security.Jwt;

/// <summary>
/// Configurações para autenticação JWT (JSON Web Token)
///
/// JWT é usado para autenticar requisições HTTP de forma stateless
/// O token contém claims (informações) sobre o usuário e é assinado digitalmente
///
/// Estrutura de um JWT:
/// - Header: Algoritmo de assinatura (HS256, RS256, etc.)
/// - Payload: Claims (sub, email, role, etc.)
/// - Signature: Assinatura digital para verificar integridade
///
/// Fluxo de autenticação baseado no schema SQL:
///
/// 1. Login (POST /api/auth/login):
///    - Usuário envia email + senha
///    - Sistema valida em users.asp_net_users
///    - Gera JWT token com claims do usuário
///    - Gera refresh token e salva em users.sessions
///    - Retorna: { accessToken, refreshToken, expiresIn }
///
/// 2. Requisições autenticadas:
///    - Cliente envia: Authorization: Bearer {accessToken}
///    - ASP.NET Core valida assinatura e expirição
///    - Claims são extraídos e disponibilizados via ICurrentUser
///
/// 3. Refresh token (POST /api/auth/refresh):
///    - Cliente envia refresh token (quando access token expirar)
///    - Sistema valida em users.sessions
///    - Gera novo access token
///    - Atualiza last_activity_at em users.sessions
///
/// Exemplo de configuração no appsettings.json:
/// {
///   "Jwt": {
///     "Secret": "sua-chave-secreta-super-segura-com-no-minimo-32-caracteres",
///     "Issuer": "BCommerce",
///     "Audience": "BCommerce.API",
///     "ExpirationInMinutes": 60,
///     "RefreshTokenExpirationInDays": 7,
///     "ValidateIssuer": true,
///     "ValidateAudience": true,
///     "ValidateLifetime": true,
///     "ValidateIssuerSigningKey": true,
///     "ClockSkew": 5
///   }
/// }
///
/// Produção (usar variáveis de ambiente):
/// JWT__Secret="{valor-do-azure-key-vault}"
/// </summary>
public sealed class JwtOptions
{
    /// <summary>
    /// Seção de configuração no appsettings.json
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// Chave secreta usada para assinar e validar tokens
    /// IMPORTANTE: Deve ter pelo menos 256 bits (32 caracteres) para HS256
    /// Em produção: usar Azure Key Vault, AWS Secrets Manager, etc.
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// Emissor do token (quem criou o token)
    /// Geralmente o nome da aplicação
    /// Claim: "iss" (issuer)
    /// </summary>
    public string Issuer { get; set; } = "BCommerce";

    /// <summary>
    /// Audiência do token (para quem o token foi criado)
    /// Geralmente o nome da API
    /// Claim: "aud" (audience)
    /// </summary>
    public string Audience { get; set; } = "BCommerce.API";

    /// <summary>
    /// Tempo de expiração do access token em minutos (padrão: 60 minutos)
    /// Recomendação: 15-60 minutos
    /// Tokens de curta duração são mais seguros
    /// </summary>
    public int ExpirationInMinutes { get; set; } = 60;

    /// <summary>
    /// Tempo de expiração do refresh token em dias (padrão: 7 dias)
    /// Usado para obter novos access tokens sem fazer login novamente
    /// Armazenado em users.sessions com expires_at
    /// </summary>
    public int RefreshTokenExpirationInDays { get; set; } = 7;

    /// <summary>
    /// Indica se deve validar o emissor (issuer)
    /// Produção: true (recomendado)
    /// Desenvolvimento: pode ser false para facilitar testes
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    /// Indica se deve validar a audiência (audience)
    /// Produção: true (recomendado)
    /// </summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    /// Indica se deve validar o tempo de vida do token (expiration)
    /// Sempre deve ser true em produção
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;

    /// <summary>
    /// Indica se deve validar a chave de assinatura
    /// Sempre deve ser true
    /// </summary>
    public bool ValidateIssuerSigningKey { get; set; } = true;

    /// <summary>
    /// Tolerância de tempo para validação de expiração (em minutos)
    /// Compensa diferenças de relógio entre servidores
    /// Padrão: 5 minutos
    /// </summary>
    public int ClockSkewMinutes { get; set; } = 5;

    /// <summary>
    /// Indica se deve salvar o token no AuthenticationProperties
    /// Útil para cenários de servidor (não SPA)
    /// </summary>
    public bool SaveToken { get; set; } = false;

    /// <summary>
    /// Indica se deve incluir detalhes de erro em caso de falha de autenticação
    /// Desenvolvimento: true (para debugging)
    /// Produção: false (por segurança)
    /// </summary>
    public bool IncludeErrorDetails { get; set; } = false;
}
