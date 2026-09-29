namespace BibliotecaApi.Exceptions;

/// <summary>O recurso solicitado não existe (HTTP 404).</summary>
public class NotFoundException(string message) : Exception(message);

/// <summary>A operação conflita com o estado atual dos dados (HTTP 409).</summary>
public class ConflictException(string message) : Exception(message);

/// <summary>Os dados enviados violam uma regra de negócio (HTTP 400).</summary>
public class BusinessRuleException(string message) : Exception(message);
