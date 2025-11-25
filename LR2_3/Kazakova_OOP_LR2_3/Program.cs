using System;
using System.Collections.Generic;
using System.Linq;

namespace Kazakova_OOP_LR2_3
{
    // ==========
    // 1. ЗАПРОС И ОТВЕТ
    // ==========
    public class Request
    {
        public string ServiceName { get; set; } = string.Empty;
        public int PayloadSize { get; set; }
        public int? DeadlineMs { get; set; }

        public Request(string serviceName, int payloadSize, int? deadlineMs = null)
        {
            ServiceName = serviceName;
            PayloadSize = payloadSize;
            DeadlineMs = deadlineMs;
        }
    }

    public class Response
    {
        public bool IsSuccess { get; set; }
        public int LatencyMs { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }

        public Response(bool isSuccess, int latencyMs, string? errorCode = null, string? errorMessage = null)
        {
            IsSuccess = isSuccess;
            LatencyMs = latencyMs;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }
    }

    // ==========
    // 2. ИНТЕРФЕЙС СЕРВИСА
    // ==========
    public interface IService
    {
        string Name { get; }
        int BaseLatencyMs { get; }
        double FailureProbability { get; }
        Response Process(Request request);
    }

    // ==========
    // 3. БАЗОВЫЙ КЛАСС СЕРВИСА
    // ==========
    public abstract class ServiceBase : IService
    {
        public string Name { get; protected set; }
        public int BaseLatencyMs { get; protected set; }
        public double FailureProbability { get; protected set; }
        protected readonly Random _random = new Random();

        protected ServiceBase(string name, int baseLatencyMs, double failureProbability)
        {
            Name = name;
            BaseLatencyMs = baseLatencyMs;
            FailureProbability = failureProbability;
        }

        public abstract Response Process(Request request);

        protected virtual void Log(Request request, Response response)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {Name}: {request.PayloadSize}B -> " +
                              $"{(response.IsSuccess ? "OK" : "FAIL")} ({response.LatencyMs}ms)");
        }
    }

    public class FastService : ServiceBase
    {
        public FastService() : base("FastService", baseLatencyMs: 50, failureProbability: 0.05) { }

        public override Response Process(Request request)
        {
            int latency = BaseLatencyMs + _random.Next(-10, 20);
            latency = Math.Max(1, latency);

            bool isSuccess = _random.NextDouble() > FailureProbability;
            var response = new Response(isSuccess, latency,
                isSuccess ? null : "ERR_FAST",
                isSuccess ? null : "FastService failed");

            Log(request, response);
            return response;
        }
    }

    public class SlowService : ServiceBase
    {
        public SlowService() : base("SlowService", baseLatencyMs: 200, failureProbability: 0.15) { }

        public override Response Process(Request request)
        {
            int latency = BaseLatencyMs + _random.Next(-30, 60);
            latency = Math.Max(10, latency);

            bool isSuccess = _random.NextDouble() > FailureProbability;
            var response = new Response(isSuccess, latency,
                isSuccess ? null : "ERR_SLOW",
                isSuccess ? null : "SlowService failed");

            Log(request, response);
            return response;
        }
    }

    // ==========
    // 4. МЕТРИКИ И СБОРЩИК
    // ==========
    public class ServiceMetrics
    {
        public string ServiceName { get; }
        public int TotalRequests { get; private set; }
        public int SuccessfulRequests { get; private set; }
        public int FailedRequests { get; private set; }
        public double AverageLatencyMs { get; private set; }
        public int MaxLatencyMs { get; private set; }

        private double _totalLatency = 0;

        public ServiceMetrics(string serviceName)
        {
            ServiceName = serviceName;
        }

        public double ErrorRate => TotalRequests == 0 ? 0.0 : (double)FailedRequests / TotalRequests;

        public void Update(Response response)
        {
            TotalRequests++;
            if (response.IsSuccess)
                SuccessfulRequests++;
            else
                FailedRequests++;

            _totalLatency += response.LatencyMs;
            AverageLatencyMs = _totalLatency / TotalRequests;
            if (response.LatencyMs > MaxLatencyMs)
                MaxLatencyMs = response.LatencyMs;
        }
    }

    public interface IMetricsCollector
    {
        void RegisterService(IService service);
        void Record(Request request, Response response);
        IReadOnlyCollection<ServiceMetrics> GetCurrentMetrics();
        event Action<ServiceMetrics>? OnMetricsUpdated;
    }

    public class InMemoryMetricsCollector : IMetricsCollector
    {
        private readonly Dictionary<string, ServiceMetrics> _metricsByService = new();

        public event Action<ServiceMetrics>? OnMetricsUpdated;

        public void RegisterService(IService service)
        {
            if (!_metricsByService.ContainsKey(service.Name))
            {
                _metricsByService[service.Name] = new ServiceMetrics(service.Name);
            }
        }

        public void Record(Request request, Response response)
        {
            if (_metricsByService.TryGetValue(request.ServiceName, out var metrics))
            {
                metrics.Update(response);
                OnMetricsUpdated?.Invoke(metrics);
            }
        }

        public IReadOnlyCollection<ServiceMetrics> GetCurrentMetrics() =>
            _metricsByService.Values;
    }

    // ==========
    // 5. ОЦЕНКА «ЗДОРОВЬЯ»
    // ==========
    public enum ServiceHealth
    {
        Healthy, Degraded, Unhealthy
    }

    public class ServiceHealthEvaluator
    {
        public double MaxHealthyErrorRate { get; set; } = 0.05;
        public double MaxDegradedErrorRate { get; set; } = 0.20;
        public int MaxHealthyLatencyMs { get; set; } = 150;
        public int MaxDegradedLatencyMs { get; set; } = 400;

        public ServiceHealth Evaluate(ServiceMetrics metrics)
        {
            bool highError = metrics.ErrorRate > MaxDegradedErrorRate;
            bool highLatency = metrics.AverageLatencyMs > MaxDegradedLatencyMs;

            if (highError || highLatency)
                return ServiceHealth.Unhealthy;

            bool mediumError = metrics.ErrorRate > MaxHealthyErrorRate;
            bool mediumLatency = metrics.AverageLatencyMs > MaxHealthyLatencyMs;

            if (mediumError || mediumLatency)
                return ServiceHealth.Degraded;

            return ServiceHealth.Healthy;
        }
    }

    // ==========
    // 6. СИСТЕМА ОПОВЕЩЕНИЙ (Вариант 8)
    // ==========
    public class Alert
    {
        public string Message { get; }
        public string ServiceName { get; }
        public DateTime Timestamp { get; }

        public Alert(string message, string serviceName)
        {
            Message = message;
            ServiceName = serviceName;
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss.fff}] ALERT [{ServiceName}]: {Message}";
        }
    }

    public class Alerting
    {
        private readonly List<Alert> _alerts = new();
        private readonly double _errorThreshold; // доля ошибок для алерта
        private readonly int _minRequests;       // минимум запросов перед анализом

        public IReadOnlyList<Alert> Alerts => _alerts.AsReadOnly();

        public Alerting(double errorThreshold = 0.1, int minRequests = 5)
        {
            _errorThreshold = errorThreshold;
            _minRequests = minRequests;
        }

        public void OnMetricsUpdatedHandler(ServiceMetrics metrics)
        {
            if (metrics.TotalRequests < _minRequests) return;

            if (metrics.ErrorRate > _errorThreshold)
            {
                var alertMsg = $"High error rate: {metrics.ErrorRate:P1} (> {_errorThreshold:P1})";
                _alerts.Add(new Alert(alertMsg, metrics.ServiceName));
                Console.WriteLine($"⚠️ {alertMsg} for {metrics.ServiceName}");
            }

            var health = new ServiceHealthEvaluator().Evaluate(metrics);
            if (health == ServiceHealth.Unhealthy)
            {
                var alertMsg = "Service status: UNHEALTHY";
                _alerts.Add(new Alert(alertMsg, metrics.ServiceName));
                Console.WriteLine($"🚨 {alertMsg} for {metrics.ServiceName}");
            }
        }
    }

    // ==========
    // 7. ГЛАВНЫЙ МОДУЛЬ
    // ==========
    public class Program
    {
        public static void Main(string[] args)
        {
            var fastService = new FastService();
            var slowService = new SlowService();

            var metricsCollector = new InMemoryMetricsCollector();
            metricsCollector.RegisterService(fastService);
            metricsCollector.RegisterService(slowService);

            var healthEvaluator = new ServiceHealthEvaluator();

            // Система оповещений 
            var alerting = new Alerting(errorThreshold: 0.1, minRequests: 3);
            metricsCollector.OnMetricsUpdated += alerting.OnMetricsUpdatedHandler;

            var random = new Random();
            var services = new IService[] { fastService, slowService };
            var requests = new List<Request>();

            // Генерация 80 случайных запросов
            for (int i = 0; i < 80; i++)
            {
                var service = services[random.Next(services.Length)];
                int payload = random.Next(50, 500);
                int? deadline = random.Next(1, 10) == 1 ? (int?)random.Next(100, 500) : null;
                requests.Add(new Request(service.Name, payload, deadline));
            }

            // Обработка запросов
            foreach (var request in requests)
            {
                IService service = request.ServiceName == fastService.Name ? fastService : slowService;
                Response response = service.Process(request);
                metricsCollector.Record(request, response);

                // Периодический вывод (каждые 20 запросов)
                if ((requests.IndexOf(request) + 1) % 20 == 0)
                {
                    Console.WriteLine("\n--- Промежуточные метрики ---");
                    PrintMetrics(metricsCollector.GetCurrentMetrics(), healthEvaluator);
                }
            }

            // Финальный вывод
            Console.WriteLine("\n=== ФИНАЛЬНЫЕ МЕТРИКИ ===");
            PrintMetrics(metricsCollector.GetCurrentMetrics(), healthEvaluator);

            // Вывод оповещений
            Console.WriteLine("\n=== ОПОВЕЩЕНИЯ ===");
            if (alerting.Alerts.Count == 0)
            {
                Console.WriteLine("Нет оповещений.");
            }
            else
            {
                foreach (var alert in alerting.Alerts)
                {
                    Console.WriteLine(alert);
                }
            }
        }

        static void PrintMetrics(IReadOnlyCollection<ServiceMetrics> metricsList, ServiceHealthEvaluator evaluator)
        {
            foreach (var m in metricsList)
            {
                var health = evaluator.Evaluate(m);
                Console.WriteLine($"{m.ServiceName}: {m.TotalRequests} req, " +
                                  $"err={m.ErrorRate:P1}, avgLat={m.AverageLatencyMs:F1}ms, " +
                                  $"maxLat={m.MaxLatencyMs}ms, status={health}");
            }
        }
    }
}
