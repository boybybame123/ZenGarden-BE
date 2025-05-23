namespace ZenGarden.Core.Services;

public class CircuitBreaker(int failureThreshold, TimeSpan resetTimeout)
{
    private int _failureCount;
    private DateTime _lastFailureTime;
    private bool _isOpen;

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
    {
        if (_isOpen)
        {
            if (DateTime.UtcNow - _lastFailureTime > resetTimeout)
            {
                _isOpen = false;
                _failureCount = 0;
            }
            else
            {
                throw new CircuitBreakerOpenException("Circuit breaker is open");
            }
        }

        try
        {
            var result = await action();
            _failureCount = 0;
            return result;
        }
        catch (Exception)
        {
            _failureCount++;
            _lastFailureTime = DateTime.UtcNow;

            if (_failureCount >= failureThreshold)
            {
                _isOpen = true;
            }

            throw;
        }
    }
}

public class CircuitBreakerOpenException(string message) : Exception(message); 