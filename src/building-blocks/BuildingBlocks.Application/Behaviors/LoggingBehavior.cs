using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Application.Behaviors;

/// <summary>
/// Pipeline behavior para log automático de requisições (comandos e queries)
///
/// Registra informações sobre cada operação executada:
/// - Nome do comando/query
/// - Tempo de execução
/// - Sucesso ou falha
/// - Erros que ocorreram
///
/// Útil para:
/// - Debugging e troubleshooting
/// - Monitoramento de performance
/// - Auditoria de operações
/// - Análise de uso do sistema
///
/// Exemplos de logs gerados baseados no schema SQL:
///
/// "[INFO] Executando CriarProdutoCommand..."
/// "[INFO] CriarProdutoCommand executado com sucesso em 125ms"
///
/// "[INFO] Executando ProcessarPagamentoCommand..."
/// "[ERROR] ProcessarPagamentoCommand falhou em 3542ms: ERRO_GATEWAY - Erro ao comunicar com gateway"
///
/// "[INFO] Executando ObterProdutoPorIdQuery..."
/// "[WARN] ObterProdutoPorIdQuery completado em 89ms com erro: PRODUTO_NAO_ENCONTRADO"
///
/// "[INFO] Executando ListarPedidosDoUsuarioQuery..."
/// "[INFO] ListarPedidosDoUsuarioQuery executado com sucesso em 234ms"
///
/// Logs lentos (> 3 segundos):
/// "[WARN] OPERAÇÃO LENTA: AtualizarEstoqueCommand levou 4523ms para executar"
/// </summary>
/// <typeparam name="TRequest">Tipo da requisição</typeparam>
/// <typeparam name="TResponse">Tipo da resposta</typeparam>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Executando {RequestName}...",
            requestName);

        try
        {
            // Executa a requisição
            var response = await next();

            stopwatch.Stop();
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            // Verifica se é uma operação lenta (> 3 segundos)
            if (elapsedMilliseconds > 3000)
            {
                _logger.LogWarning(
                    "OPERAÇÃO LENTA: {RequestName} levou {ElapsedMilliseconds}ms para executar",
                    requestName,
                    elapsedMilliseconds);
            }

            // Log de sucesso ou falha
            LogarResultado(requestName, response, elapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Exceção não tratada ao executar {RequestName} após {ElapsedMilliseconds}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    /// <summary>
    /// Loga o resultado da operação (sucesso ou falha)
    /// </summary>
    private void LogarResultado(string requestName, TResponse response, long elapsedMilliseconds)
    {
        // Se a resposta é um Result, loga informações específicas
        if (response is Models.Result result)
        {
            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "{RequestName} executado com sucesso em {ElapsedMilliseconds}ms",
                    requestName,
                    elapsedMilliseconds);
            }
            else
            {
                _logger.LogWarning(
                    "{RequestName} completado em {ElapsedMilliseconds}ms com erro: {ErrorCode} - {ErrorMessage}",
                    requestName,
                    elapsedMilliseconds,
                    result.Error.Code,
                    result.Error.Message);
            }
        }
        else
        {
            // Para respostas que não são Result
            _logger.LogInformation(
                "{RequestName} executado em {ElapsedMilliseconds}ms",
                requestName,
                elapsedMilliseconds);
        }
    }
}
