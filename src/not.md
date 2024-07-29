# eShop Mikroservis Projesi

## Servislerin Port Numaraları

| **Mikroservis**  | Local Env.  | Docker Env.  | Docker Inside  |
|---|---|---|---|
| Catalog   | 5000-5050  | 6000-6060  | 8000-8081  |
| Basket  | 5001-5051  | 6001-6061  | 8080-8081  |
| Discount  | 5002-5052  | 6002-6062  | 8080-8081  |
| Ordering  | 5003-5053  | 6003-6063  | 8080-8081  |

- port numaraları http/https olarak ayrılmıştır
- Docker Inside'da port numaralarının aynı olmasının sebebi, bu numaraların dockerfile'dan gelecek olması. Tüm .NET projeleri için geçerlidir

---

## Catalog Service

### Projenin Temelinin Atılması ve Docker Konfigürasyonları

1. Portları _launchSettings.json_ dosyasında **http** ve **https** bölümlerine ait **applicationUrl** kısımlarını değiştirerek ayarladık.

1. Bu serviste **Vertical Slice Architecture** kullanacağız. Bu mimari hem makro hem de mikro düzeyde uygulanabilir. Features adında bir klasör tanımlayıp ardından her modele özgü klasör açıp bu klasörde operasyonları tanımlayarak bu mimariyi sağlayabiliriz. Bu servis özelinde "**Products**" klasörü bunu sağlamamızda etkili olacak.

1. İlk handler sınıfımız olan ```CreateProductHandler```'da ek olarak bu iş sınıfının modelini de record olarak tanımlıyoruz. Klasik **CQRS pattern**'ü uygulayacağız.

1. **BuildingBlocks** adı verilen bir class library tanımladık. Burada tüm projemize etkiyecek soyutlamaları, konfigürasyonları ve diğer bazı genellemeleri uygulayacağız. (Common project)

1. **ICommand** interface'ini oluşturduk. Burada kullandığımız ```public interface ICommand : IRequest<Unit>``` tanımlamamız, bize void dönüşlü metotları bu interface'i implement ettikten sonra kullanabilmemize olanak tanıyacak. **Unit**, CQRS'e ait özel bir sınıftır ve void tipi ifade eder.

1. **IQuery** interface'ini oluşturduk. Query'leri soyutladık ve **notnull** **constraint**'i uygulayarak null değer dönmeyi engelledik.

1. **ICommandHandler** interface'inin oluşturulması

   - ```public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>``` </br></br> Burada CQRS'e ait bir interface olan **IRequestHandler**'dan kalıtım aldık. **TCommand** generic yapısının ancak ve ancak, kendi oluşturduğumuz **ICommand** arayüzüne eşit olmasını sağladık. Ayrıca **notnull constraint** ekleyerek  null dönememesini sağladık.

   - ```public interface ICommandHandler<in TCommand> : ICommandHandler<TCommand, Unit> where TCommand : ICommand<Unit>``` </br></br> Az evvel oluşturmuş olduğumuz **ICommandHandler** interface'inin farklı bir türünü tanımladık. Bu generic tür; geriye hiçbir şey dönmez ve **TCommand** olarak Unit dönüşlü bir **ICommand** kabul eder.

1. **IQueryHandler** interface'inin oluşturulması

   - ```public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>``` </br></br> Bir önceki adımın benzeri bir tanımlama uyguladık. TRequest parametresi olarak IQuery alan ve notnull constraint'ine sahip bir interface tanımladık.

 1. **IEntityBuilder** interface'inin oluşturulması ve implementasyonu; 
	- Burada eğitimin haricinde kendim bir şeyler kattım. **Reflection** kullanarak generic bir entity builder oluşturdum ve bu şekilde command'dan entity üretimini sağladım. Custom mapping gibi de düşünebiliriz.

 1. **CreateProductEndpoint** sınıfının oluşturulması; 
	- Bu serviste minimal API ile çalışacağımız için endpoint'lerden faydalanacağız.
	- .NET Endpointleri için yazılmış olan **Carter** kütüphanesini projeye dahil ettik.
	- Oluşturduğumuz **CreateProductEndpoint** sınıfına, **ICarterModule** interface'ini implement ettik.
	- Mapping işlemi için **Mapster** kütüphanesini kurduk ve **Adapt** metodu ile mapleme işlemini gerçekleştirip response döndürdük.
	- Handler sınıfımıza **IDocumentSession**'dan bir örnek enjekte ettik. Bu sayede Marten kütüphanesinin sunmuş olduğu özelliklere erişebildik. **Store** metodu ile DB'ye ekleme işlemi yaptık ve **SaveChangesAsync** metodu ile de DB'de yaptığımız bu yeniliği kaydettik.
	
1. Serviste yaygın bir şekilde kullanacağımız kütüphanelerin entegrasyonunu kolaylaştırmak için **GlobalUsing** adında bir sınıf tanımlayıp kütüphanelerin kullanımını globalize ettik.

1. Projemize ait bir **docker-compose** projesi oluşturduk. Bunu yapmak için projeye sağ tıklayıp **Container Orchestrator Support**'tan Linux'u seçip onayladık.

   1. **PostgreSQL** veri tabanına bağlanacağımız için docker ayarları burada kritik önem arz ediyor. (buralar tekrar edilecek)
   1. **docker-compose.yaml** ve **docker-compose.override.yaml** dosyalarını PostgreSQL bilgilerini içerecek şekilde düzenledik ve kaydettik.
   1. Proje ortamını dockerize etmek için **docker-compose** projesini Visual Studio'dan ayağa kaldırıyoruz. Tüm imajlar ve çevre değişkenleri otomatik olarak yükleniyor. (Çalışmadı, VS'i tekrar başlatınca düzeldi)
   1. Docker Desktop'tan container detaylarına baktığımızda artık PostgreSQL'i **catalogdb** ismi ile görebilmekteyiz. Terminalden ```docker ps``` komutu ile container'ın çalıştığını doğruladık.
   1. ```docker exec -it ;id; bash``` komutu ile PostgreSQL'in bulunduğu container'daki bash script'e ulaştık.
   1. Bulunduğumuz bash'te ```psql -U postgres``` komutunu çağırarak PostgreSQL'e has olan shell script'e ulaştık.
   1. ```\l``` ile tüm db'leri listeleriz.

   | Name      | Owner    | Encoding | Locale Provider | Collate    | Ctype      | ICU Locale | ICU Rules | Access privileges |
|-----------|----------|----------|-----------------|------------|------------|------------|-----------|-------------------|
| CatalogDb | postgres | UTF8     | libc            | en_US.utf8 | en_US.utf8 |            |           |                   |
| postgres  | postgres | UTF8     | libc            | en_US.utf8 | en_US.utf8 |            |           |                   |
| template0 | postgres | UTF8     | libc            | en_US.utf8 | en_US.utf8 |            |           | =c/postgres      +|
|           |          |          |                 |            |            |            |           | postgres=CTc/postgres |
| template1 | postgres | UTF8     | libc            | en_US.utf8 | en_US.utf8 |            |           | =c/postgres      +|
|           |          |          |                 |            |            |            |           | postgres=CTc/postgres |

   8. ```\c CatalogDb``` komutu ile CatalogDb'ye bağlanırız. Şuan bir tablo bulunmadığı için DB boş gözükecektir. Veri girişinin ardından Marten kütüphanesi bizim yerimize code-first yaklaşımı ile tüm yapıyı oluşturacaktır.
   9. Postman üzerinden istek atarız ve başarılı sonucu alırız. *5432 portu daha önce kullanımda olduğu için sıkça hata alındı. Sonuç olarak düzeltildi.* 
   10. DB'de ```\d``` komutunu çağırarak Marten tarafından oluşturulmuş olan tablomuzu görebilir ve tablo üzerinde SQL sorguları çağırabiliriz.

  1. **GetProduct, GetProductById, GetProductByCategory, UpdateProduct, DeleteProduct** operasyonları için Handler ve Endpoint sınıflarının oluşturulması


### Cross-Cutting Concerns uygulaması

- **Cross-Cutting Concerns**, request ve response geçişlerinde farklı operasyonları sırayla yürüterek daha modüler bir ilerleme ortaya koymamızı sağlayan bir yaklaşımdır.
- Validation, Caching, Logging gibi ara operasyonları daha kolay yönetilebilir ve uygulanabilir hale getirmeyi sağlayan CCC'yi uygulamak için **MediatR Pipeline Behaviors** üzerinden bazı implementasyonlarda bulunacağız.

#### Validation
- Öncelikle bir Validator tanımlayalım ve validate durumunu CCC kullanmaksızın Handle metodunda ele alalım;

``` 
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(x => x.Category).NotEmpty().WithMessage("Category is required");
        RuleFor(x => x.ImageFile).NotEmpty().WithMessage("ImageFile is required");
        RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than 0");
    }
}

internal class CreateProductCommandHandler
    (IDocumentSession session, IValidator<CreateProductCommand> validator)
    : ICommandHandler<CreateProductCommand, CreateProductResult> // sadece o assembly'de geçerli olması için internal yaptık çünkü başka bir yerde çağırmayacağız
{
    public async Task<CreateProductResult> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(command, cancellationToken);
        var errors = result.Errors.Select(x => x.ErrorMessage).ToList();
        if (errors.Any())
        {
            throw new ValidationException(errors.FirstOrDefault());
        }
        .
        .
    }
}
```

- Bu durumu tekrarlı bir şekilde uygulamak kod kalabalığını ve bakım zorluğunu beraberinde getirecektir.

- Bunu önlemek için ValidationBehavior adında bu işlemi her request için yönetme kabiliyetine sahip bir sınıf oluşturduk.
  1. Type tanımlaması

      ```csharp
      public class ValidationBehavior<TRequest, TResponse> (IEnumerable<IValidator<TRequest>> Validators)
      : IPipelineBehavior<TRequest, TResponse>
      where TRequest : ICommand<TResponse> 
      ```

      <ul style="list-style-type:square;">
        <li>Bu type MediatR'ün bir interface'i olan <b>IPipelineBehavior</b>'dan kalıtım alıyor.</li>
        <li>Request alıp geriye Response dönmeyi belirten bir generic yapı olarak tanımlamayı yaptık.</li>
        <li>Parametre olarak Request'in tüm validator'larını IEnumerable şeklinde alıyoruz.</li>
        <li>Request'i sadece ICommand'a eşit olmak üzere belirliyoruz. Çünkü query'lerde validation operasyonuna ihtiyacımız henüz yok.</li>
      </ul>
  
  1. İmplementasyon
 

        ```
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse>/next, CancellationToken cancellationToken)
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(Validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .Where(r => r.Errors.Any())
                .SelectMany(r => r.Errors)
                .ToList();

            if(failures.Any())
            {
                throw new ValidationException(failures);
            }

            return await next();
        }
        ```

      <ul style="list-style-type:square;">
        <li>Öncelikle request'in valide edilecek halini ifade eden bir ValidationContext instance'ı oluşturuyoruz.</li>
        <li>Bu instance'ı tüm Validator'ları dönerken kullandığımız ValidateAsync'de parametre olarak veriyoruz.</li>
        <li>Metot sonucunu itere ediyoruz ve herhangi bir hata oluştuysa bu hataları bir listeye alıyoruz.</li>
        <li>Eğer hata varsa programın devam etmesini önlemek için hata fırlatıyoruz, hata yoksa ise pipeline'ın bir sonraki bileşeninden devam ediyoruz.</li>
      </ul>

#### Exception Handling

1. Exception yönetimini proje bazında yönetebilmek için serviste bulunan Program.cs'e Exception Handler yazdık. Bu sayede istek işlenirken herhangi bir hata gelip gelmediğini kontrol edebildik ve eğer hata varsa bu hatayı response olarak döndürdük;

    ```csharp
    app.UseExceptionHandler(exceptionHandlerApp =>
    {
        exceptionHandlerApp.Run(async context =>
        {
            var exception = context.Features.Get<IExceptionHandlerPathFeature>()?.Error; // isteğin path'i ve eğer varsa exception

            if (exception is null) // exception yoksa dönüş yap
            {
                return;
            }

            var problemDetails = new ProblemDetails
            {
                Title = exception.Message,
                Status = StatusCodes.Status500InternalServerError,
                Detail = exception.StackTrace
            };
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>(); // logger'ı servislerden aldık
            logger.LogError(exception, exception.Message);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(problemDetails);
        });
    });
    ```
2. Global Exception Handling'e hazırlık için her nesne ve durum için kullanabileceğimiz Custom Exception'lar tanımladık. (BadRequest, NotFound, InternalServer exceptions)

3. Global Exception Handling'i opere edecek sınıfımız olan CustomExceptionHandler'ı oluşturduk.

    ```csharp
    public class CustomExceptionHandler 
        (ILogger<CustomExceptionHandler> logger)
        : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogError(
                "Error Message: {ExceptionMessage}, Time of occurrence {Time}", 
                exception.Message, DateTime.UtcNow); // hata mesajını ve hata tarihini log'a yazdır

            (string Detail, string Title, int StatusCode) details = exception switch // TODO: burada daha etkin bir yapı kullanabilir miydik bilmiyorum
            {
                InternalServerException =>
                (
                    exception.Message,
                    exception.GetType().Name,
                    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError
                ),
                ValidationException =>
                (
                    exception.Message,
                    exception.GetType().Name,
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest
                ),
                BadRequestException =>
                (
                    exception.Message,
                    exception.GetType().Name,
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest
                ),
                NotFoundException =>
                (
                    exception.Message,
                    exception.GetType().Name,
                    httpContext.Response.StatusCode = StatusCodes.Status404NotFound
                ),
                _ =>
                (
                    exception.Message,
                    exception.GetType().Name,
                    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError
                )
            };

            var problemDetails = new ProblemDetails
            {
                Title = details.Title,
                Detail = details.Detail,
                Status = details.StatusCode,
                Instance = httpContext.Request.Path
            };

            problemDetails.Extensions.Add("traceId", httpContext.TraceIdentifier);

            if (exception is ValidationException validationException) 
            {
                problemDetails.Extensions.Add("ValidationErrors", validationException.Errors); // Validasyona dair hata(lar) oluştuysa bunu ValidationErrors başlığına ekle
            }

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: cancellationToken); // response'u hata bilgisi ile doldur
            return true;
        }
    }

    ```

    <ul style="list-style-type:square;">
    <li>Oluşturduğumuz sınıfa, .NET'e dahili olan ve hata yönetimini sağlayan <b>IExceptionHandler</b> sınıfını implemente ediyoruz.</li>
    <li>Hata başlığı, detayı ve durum kodunu tutacak bir tuple tanımlıyoruz.</li>
    <li>Bu tuple'ı, uyguladığımız switch koşulunda dolduruyoruz.</li>
    <li>Hazır ProblemDetails sınıfından, tuple'daki değerleri kullanarak bir instance oluşturuyoruz. Eğer bu hata bir validasyon hatasıysa bu hata/ların listesini de ekliyoruz.</li>
    <li>Elde ettiğimiz ProblemDetails nesnesini, response içeriğine yazıyoruz.</li>
    <li>Son işlem olarak; oluşturduğumuz sınıfı Program.cs'te hem servis hem de uygulama tarafında konfigüre ediyoruz.</li>
    </ul>

#### Logging

1. Dikkat edersek, handler sınıflarımızın primary ctor'larının her birine ILogger sınıfından bir instance dahil ettik. Ve yine dikkat edersek, oluşturduğumuz her handler sınıfı için bunu yinelemek zorundayız. Dolayısıyla bu yol gereksiz bir kod tekrarına yol açar. Bunu önlemek için yine **MediatR Pipelive Behaviour** davranışından faydalanacağız.
1. BuildingBlocks projesinde LoggingBehavior adında bir sınıf tanımlıyoruz;

    ```csharp
    public class LoggingBehavior<TRequest, TResponse> 
        (ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull, IRequest<TResponse>
        where TResponse : notnull
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            logger.LogInformation("[START] Handle request={Request} - Response={Response} - RequestData{RequestData}", typeof(TRequest).Name, typeof(TResponse).Name, request);

            var timer = new Stopwatch();
            timer.Start();

            var response = await next();

            timer.Stop();
            var timeTaken = timer.Elapsed;

            if(timeTaken.Seconds > 3) // isteğin işlenmesi 3 saniyeden uzun sürmüşse
            {
                logger.LogWarning("[PERFORMANCE] The request {Request} took {TimeTaken} seconds.", typeof(TRequest).Name, timeTaken.Seconds);
            }

            logger.LogInformation("[END] Handled {Request} with {Response}", typeof(TRequest).Name, typeof(TResponse).Name);
            return response;
        }
    }
    ```
    <ul style="list-style-type:square;">
    <li>Oluşturduğumuz sınıfa MediatR sınıfının interface'i olan IPipelineBehavior'u implemente ediyoruz ki, bir middleware davranışı gösterilebilsin..</li>
    <li>Bu sınıfın primary ctor'una ILogger interface'inden bir instance ekliyoruz. Bu, logging işlemini tek bir noktadan uygulamamıza yarayacak.</li>
    <li>İstek geldiği an, LogInformation metodunu uygulayarak isteği logluyoruz.</li>
    <li>Bu loglama işleminden sonra, isteğin ne kadar sürede işlendiğini hesaplamamızı sağlayacak olan timer adında bir değişken tanımlıyoruz. Bu değişken Stopwatch sınıfından bir instance teşkil ediyor. Response'u beklemeden önce de, timer'ı başlatıyoruz.</li>
    <li>Response içeriğini var response = await next(); kod satırı ile bir değişkene aktarıyoruz.</li>
    <li>Response'tan bir instance oluşur oluşmaz, timer'ı durduruyoruz. Geçen süre 3 saniyeden büyükse, bu durum bir performans sorunu teşkil ettiği için warning etiketiyle bu durumu logluyoruz.</li>
    <li>Son olarak, response gönderilmeden hemen önce request'ın işlenmesinin bittiğini işaret edecek şekilde [END] belirteciyle bir loglama yapıyoruz</li>
    </ul>
1. Oluşturduğumuz bu behavior'ı, ```config.AddOpenBehavior(typeof(LoggingBehavior<,>));``` kodu ile Program.cs'te MediatR ayarlarına register ediyoruz. 



### Data Seeding uygulaması

- Marten kütüphanesi ile Data Seeding eklemek için şu adımları gerçekleştirdik;
    1. CatalogInitialData ismi verilen bir sınıf oluşturduk ve bu sınıfa Marten kütüphanesinde bulunan IInitialData interface'ini implemente ettik.
    1. Bir session oluşturduk ve tabloda veri var mı, yok mu kontrol ettik.
    1. Veri yoksa oluşturduğumuz Product'ları tabloya ekledik ve değişiklikleri kaydettik
    1. Oluşturduğumuz bu sınıfı ```builder.Services.InitializeMartenWith<CatalogInitialData>();``` kodu ile Marten konfigürasyonuna dahil ettik.


### Pagination eklenmesi

- Pagination uygulamak için bir extension metot yazmak yerine Marten'in *ToPagedListAsync* metodunu kullandık. Metoda sadece sayfa numarasını ve sayfa eleman sayısını vererek; güncel sayfa, ilk/son sayfa, toplam eleman sayısı gibi verilere dönen PagedList instance'ından erişebiliyoruz. Şu şekilde bir uygulamaya gittik;

    1. *GetProductsEndpoint* sınıfında parametresiz bir şekilde bulunan ve kullanılmayan GetProductsRequest sınıfına PageNumber ve PageSize adında iki yeni parametre ekledik.
    1. Bu yeni request'ı, mapping ile Query sınıfına eşitledik. Dolayısıyla aynı değişikliği bu sınıfta da yapmamız gerekti ve sınıfa PageNumber ve PageSize parametrelerini ekledik.
    1. Handler tarafında ```var products = await session.Query<Product>()
            .ToPagedListAsync(query.PageNumber ?? 1, query.PageSize ?? 10, cancellationToken);``` tanımlaması ile PagedList nesnesini elde ettik.


### Health Check eklenmesi

- Sadece Program.cs tarafını ayarlamamız bu ayarlama için yeterli. Projede DB kritik bir yer tuttuğu için DB odaklı bir kontrol yaptık. Bunu sağlamak için ise AddNpgSql metodundan faydalandık ve connection string'i vererek ilgili DB'yi kontrol etmesini belirttik.
- Response'u JSON formatında ve daha okunaklı yazması için ise app konfigürasyonlarında options tanımladık ve burada, eklediğimiz UI kütüphanesini kullandık