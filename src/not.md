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


### .NET uygulamasının Dockerize edilmesi

- Şu ana kadarki yaptığımız dockerize işlemleri, sadece servis tarafını kapsıyordu. 
- Dockerize edilmiş bir DB kaynağından veri akışı sağlıyorduk fakat ayağa kalkan .NET uygulamamız, local olarak ayağa kalkıyordu. Bu durum bizim mikroservislerde hedeflediğimiz yaklaşım değil.
- .NET uygulaması tarafını da dockerize etmek için şu adımları uyguladık;

    1. Catalog.API projesinde Dockerfile oluşturulması
        i. Catalog.API projesinde, projeye sağ tıklayıp Add > Docker Support seçeneği ile yeni bir Dockerfile ürettik. 
        ii. Overwrite olarak ekledik çünkü ilk oluşan Dockerfile'da BuildingBlocks projesine referansımız yoktu.
          
    2. Compose işlemi için docker-compose.yaml dosyasını düzenlenmesi
        i. Şu ana kadar yaptığımız işlem sadece servisi orkestre etmekti lakin artık .NET uygulamasını da(Catalog servisi) compose etmeliyiz. 
         
        i. Catalog.API projesine sağ tıklayıp Add > Container Orchestrator Support seçeneğinden devam ettik. Bu adımla birlikte azaten var olan docker-compose projemiz overwrite ediliyor ve servisi compose etmemize yarayacak olan kodlar yeni Dockerfile sayesinde generate ediliyor.
         
        i. Dosyada yer alan "catalog.api" field'ında öncelikle ports field'ını düzenledik. 
              ```   
               ports:                       ports:
                  - "8080"       --->          - "6000:8080"
                  - "8081"                     - "6060:8081"
              ```
           İlk port numaraları Docker ortamından erişeceğimiz numaralar, ikinciler ise Docker çevre değişkenleri
        
        i. environment field'ına connection string ekledik. ``` - ConnectionStrings__Database=Server=catalogdb;Port=5433;Database=CatalogDb;User Id=postgres;Password=postgres;Include Error Detail=true ``` burada json dosyasındaki string ile aynı olmasına dikkat ediyoruz. Bunu yapmamızın sebebi connection string'i bir çevre değişkeni olarak kullanabilmek ve Catalog.API projesine enjekte etmek. İki tane alt tire ('__') kullanmanın nedeni ise, Docker Compose'un json dosyasındaki hiyerarşik yapıyı düz metin bir çevre değişkenine dönüştürmek
         
        i. depends_on field'ını ekledik. 
            ```    
           depends_on:
             - "catalogdb"
            ``` 
           Bu sayede aynı ağda yer alan docker compose servisleri olan Catalog.API ve DB, birbiriyle iletişime geçebilecek. Burada container isminin eşleşmesine dikkat edelim. 
     
     3. Compose işlemi
        i. Burada hedefimiz Catalog.API ve CatalogDb container'larının uyumlu ve sağlıklı bir şekilde çalışmasıın sağlamak. Compose uygulamak için iki yolumuz var;
            a. docker-compose projesinde terminal kullanmak
               i. Sağ tıklayıp PowerShell başlatılır
               i. ```docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d``` komutu yürütülür
            a. docker-compose projesini startup olarak ayağa kaldırmak.
        i. Bu iki yoldan birini uyguladıktan sonra, debug kısmında projemizin iki farklı container halinde ayağa kalktığını görmeliyiz. API projesi 6060 portunda çalışmalı. (hata alındı, PSQL'in port numaralarını 5433:5432'den 5432:5432 olarak değiştirince hata düzeldi) (clean solution ardından build almak hatalar konusunda etkili)

---

## Basket Service

- Bu servis genel itibariyle kullanılan veritabanı, bileşenler ve mimari olarak **Catalog** servisine benzerlik gösterecektir. (PostgreSQL, Endpoint pattern, CQRS and Mediator, VSA, HealthChecks)
- Catalog servisindeki bileşenler dışında; gRPC, Redis ve RabbitMQ araçları kullanılacaktır.
- Tasarım desenlerinde ise **Repository pattern** uygulanacaktır.

### Servise ait DB'nin docker-compose'a Dahil Edilmesi 

1. Halihazırda var olan docker-compose projemizde bulunan docker-compose.yml dosyasında, **services** ve **volumes** field'larına basketdb eklenir;
```
services:
  catalogdb:
    image: postgres

  basketdb:
    image: postgres

.
.

volumes:
  postgres_catalog:
  postgres_basket:
```

2. Aynı projenin docker-compose.override.yml dosyasında da benzer değişikliklerde bulunulur; (port numarasının farklı olmasına dikkat ediyoruz) 

```
services:
  catalogdb:
    .
    .

  basketdb:
    container_name: basketdb
    environment:
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=postgres
      - POSTGRES_DB=BasketDb
    restart: always
    ports:
        - "5433:5432"
    volumes:
      - postgres_basket:/var/lib/postgresql/data/ 
```
      
3. docker-compose projesi ayağa kaldırılır. BasketDb'yi gözlemleyebiliyorsak işlem başarılı demektir. (connection string'in uyumlu olması şarttır)

### Caching Eklenmesi

1. **Scrutor** kütüphanesinin eklenmesi
    - Bu kütüphane .NET'te built-in olarak bulunan IoC container'ı extension metotları sayesinde güçlendirir ve daha çeşitli eylemleri yapabilir hale getirir. 
    - Biz Scrutor kütüphanesini, oluşturacağımız CachedBasketRepository üzerinde **Decorator pattern** uygulamak için kullanacağız.</br></br>
    

      ```
      builder.Services.AddScoped<IBasketRepository, BasketRepository>();
      builder.Services.AddScoped<IBasketRepository, CachedBasketRepository>();
      ```
    - Bu kullanım uygun olmaz çünkü IoC'ye aynı interface için iki farklı concrete sınıf kaydettik. Böyle bir durumda compiler tarafından; IoC'ye kaydedilen son eklenen sınıf dikkate alınacak, ilk sınıf görmezden gelinecektir.</br></br>


      ```
      builder.Services.AddScoped<IBasketRepository>(provider =>
      {
          var basketRepository = provider.GetRequiredService<BasketRepository>();
          return new CachedBasketRepository(basketRepository, provider.GetRequiredService<IDistributedCache>());
      });
      ```
    - Bu sorunu aynı interface'i tek type olarak verdkten sonra provider üzerinden kayıtlı olan concrete sınıfa erişip, bu sınıfın instance'ını ve IDistributedCache'ten bir örneği alıp CachedBasketRepository'i IoC'ye bu şekilde kaydederek çözebiliriz fakat bu bize bakım zorluğunu ve karmaşıklığı beraberinde getirir.</br></br>


    - O yüzden bu işlemi tek bir metoda indirgeyebiliriz. Program.cs tarafında ```builder.Services.Decorate<IBasketRepository, CachedBasketRepository>();``` kodu ile biz şu talimatı veriyoruz; CachedBasketRepository, hem IBasketRepository'i implemente edecek hem de bu interface'ten bir örnek alabilecek.



1. CachedBasketRepository sınıfının oluşturulması
    ```

    public class CachedBasketRepository 
        (IBasketRepository repository, IDistributedCache cache)
        : IBasketRepository
    {
        public async Task<ShoppingCart> GetBasket(string userName, CancellationToken cancellationToken = default)
        {
            var cachedBasket = await cache.GetStringAsync(userName, cancellationToken);
            if(!string.IsNullOrEmpty(cachedBasket))
            {
                return JsonSerializer.Deserialize<ShoppingCart>(cachedBasket);
            }

            var basket = await repository.GetBasket(userName, cancellationToken);
            await cache.SetStringAsync(userName, JsonSerializer.Serialize(basket), cancellationToken);
            return basket;
        }

        public async Task<ShoppingCart> StoreBasket(ShoppingCart basket, CancellationToken cancellationToken = default)
        {
            await repository.StoreBasket(basket, cancellationToken);
            await cache.SetStringAsync(basket.UserName, JsonSerializer.Serialize(basket), cancellationToken);

            return basket;
        }

        public async Task<bool> DeleteBasket(string userName, CancellationToken cancellationToken = default)
        {
            return await repository.DeleteBasket(userName, cancellationToken);
        }
    }

    ```
    - Önceden Program.cs tarafında belirttiğimiz üzere Decorator pattern'i uyguladık. 
    - GetBasket için; Önce basket'in cache'lenip cache'lenmediğini kontrol ediyoruz. Cache'lendiyse direkt dönüş sağlıyoruz. Aksi takdirde ana metodu çağırıyoruz, dönen değeri cache'liyoruz ve dönüş sağlıyoruz.
    - StoreBasket için; Önce ana metodu çağırıyoruz, sonrasında ise dönen değeri cache'liyoruz ve dönüş sağlıyoruz.
    - Burada ana metotların tekrarı, **Proxy pattern** kullanımı ile ilişkilidir. </br></br>
    ```
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("Redis");
    });
    ```
    - Program.cs'te yaptığımız bu konfigürasyon ile IDistrubutedCache interface'inin Redis üzerinden bağlantı kurmasını ve metotları buna göre yürütmesini sağladık. json dosyasına bağlantı adresini ekledik ve builder.Configuration'dan bu adrese eriştik.


### Docker Operasyonları

1. Redis'in Orkestrasyonu

    - Redis'i, Docker'a her zamanki gibi docker-compose projesini düzenleyerek ekleyeceğiz.
    </br></br>
    1. docker-compose.yml dosyasına Redis için container eklenmesi
   
    ```
    services:
        .
        .
      distributedcache:
        image: redis
    ```

    2. docker-compose.override.yml dosyasına, bir önceki adımda eklenen container'ın konfigürasyonunun eklenmesi
    
    ```
    distributedcache:
      container_name: distributedcache
      restart: always
      ports:
          - "6379:6379"
    ```
    3. docker-compose projesinin Startup olarak seçilip ayağa kaldırılması
      - Bu işlemden sonra Redis container'ı başarılı bir şekilde ayağa kalkmalıdır.
      - Redis'in çalışıp çalışmadığını test etmek için container'daki *Exec* bölümünde Redis komutları çağırabiliriz.

1. API'ın Orkestrasyonu

    1. Projemize sağ tıklayıp Add>Docker Support diyoruz ve varolan Dockerfile'ı override ediyoruz.

    2. Projeye sağ tıklayıp Add>Container Orchestratator Support diyoruz ve varolan docker-compose.yml ile docker-compose.override.yml dosyalarımız bu sayede güncelleniyor;
    ```
    >docker.compose.yml


    .
    .
    basket.api:
      image: ${DOCKER_REGISTRY-}basketapi
      build:
        context: .
        dockerfile: Services/Basket/Basket.API/Dockerfile
    .
    .    
    ```

    ```
    >docker.compose.override.yml


    .
    .
    basket.api:
      environment:
        - ASPNETCORE_ENVIRONMENT=Development
        - ASPNETCORE_HTTP_PORTS=8080
        - ASPNETCORE_HTTPS_PORTS=8081
      ports:
        - "8080"
        - "8081"
      volumes:
        - ${APPDATA}/Microsoft/UserSecrets:/home/app/.microsoft/usersecrets:ro
        - ${APPDATA}/ASP.NET/Https:/home/app/.aspnet/https:ro
    .
    .    
    ```

    3. docker-compose.override.yml dosyasında şu değişiklik ve eklemelerde bulunuyoruz;
        - Projede kullanacağımız port numaraları (ports field'ı)
        - Connection string (Server=containername)
        - Bağımlılıklar (depends_on field'ı -> containernames)
    ```
    basket.api:
    environment:
        - ASPNETCORE_ENVIRONMENT=Development
        - ASPNETCORE_HTTP_PORTS=8080
        - ASPNETCORE_HTTPS_PORTS=8081
        - ConnectionStrings__Database=Server=basketdb;Port=5432;Database=BasketDb;User Id=postgres;Password=postgres;Include Error Detail=true
        - ConnectionStrings__Redis=distributedcache:6379
    depends_on:
        - basketdb
        - distributedcache
    ports:
        - "6001:8080"
        - "6061:8081"
    volumes:
        - ${APPDATA}/Microsoft/UserSecrets:/home/app/.microsoft/usersecrets:ro
        - ${APPDATA}/ASP.NET/Https:/home/app/.aspnet/https:ro   
    ```

    4. docker-compose projesinde Properties kısmında **Service Name** kısmında **basket.api** projemizi seçiyoruz ve docker-compose projesini ayağa kaldırıyoruz
     
---

## Discount Service

- Bu projede kullanılacak bileşenler; gRPC ile senkron iletişim ve proto dosyaları aracılığıyla CRUD işlemleri, Basket servisi ile kurulacak gRPC ağı, veritabanı tarafında SQLite, veritabanına erişim ve yüksek performanslı işlemler için Entity Framework Core olacaktır. Mimari olarak ise N-Katmanlı Mimari tercih edilmiştir.
- Servisin ana fikri;

    <ul style="list-style-type:square;">
    <li>Client sepetine ürün eklediğinde Basket servisi, Discount servisimizi tüketecek ve seçili ürünlerine ait indirimleri getirecek.</li>
    <li>Basket servisinden yanıt beklenecek, yani senkron bir yapı kullanılacağı için yüksek performansa oldukça özen gösteriyoruz.</li>
    <li>Parametre olarak Request'in tüm validator'larını IEnumerable şeklinde alıyoruz.</li>
    <li>Request'i sadece ICommand'a eşit olmak üzere belirliyoruz. Çünkü query'lerde validation operasyonuna ihtiyacımız henüz yok.</li>
    </ul>
- Servisimizi gRPC template'i seçerek oluşturuyoruz.