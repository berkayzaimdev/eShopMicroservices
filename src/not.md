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
    </ul>

### .proto Dosyasının oluşturulması

```
syntax = "proto3";

option csharp_namespace = "Discount.Grpc";

package discount;

service DiscountProtoService{
	rpc GetDiscount (GetDiscountRequest) returns (CouponModel);
	rpc CreateDiscount (CreateDiscountRequest) returns (CouponModel);
	rpc UpdateDiscount (UpdateDiscountRequest) returns (CouponModel);
	rpc DeleteDiscount (DeleteDiscountRequest) returns (DeleteDiscountResponse);
}

message GetDiscountRequest {
	string productName = 1;
}

message CouponModel{
	int32 id = 1;
	string productName = 2;
	string description = 3;
	int32 amount = 4;
}

message CreateDiscountRequest{
	CouponModel coupon = 1;
}

message UpdateDiscountRequest{
	CouponModel coupon = 1;
}

message DeleteDiscountRequest{
	string productName = 1;
}

message DeleteDiscountResponse{
	bool success = 1;
}
```

- syntax olarak proto3 seçtik
- option'da csharp_namespace olarak projemizin namespace'ini seçtik yani "Discount.Grpc"
- package olarak discount seçtik. Servisimiz bu paketi tüketecek o yüzden böyle bir isimlendirmede bulunduk
- service kısmında tüketilecek bütün servisleri _rpc [servis] [request] returns [response]_ formatında tanımlıyoruz
- Request ve response modelleri tanımlıyoruz. Veri tipini doğru seçmek ve verilerin numara sırasını doğru yazmak önem taşıyor
- Dosyanın "Properties" kısmında yer alan Build Action ayarını **Protobuf Compiler** olarak seçmeyi de unutmuyoruz. Bu dosya, sadece tüketim için kullanılacağından dolayı, gelen yeni seçenekte de **Server Only**'i işaretliyoruz
- Sınıfı oluşturduktan ve tüm bu işlemleri uyguladıktan sonra build alıyoruz. Generated sınıflarımızı projedeki tüm dosyalar arasında yer alan obj>Debug>net8.0>Protos klasöründe bulabiliriz

### DiscountService sınıfının oluşturulması

```
public class DiscountService : DiscountProtoService.DiscountProtoServiceBase
{
    public override Task<CouponModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
    {
        return base.GetDiscount(request, context);
    }

    public override Task<CouponModel> CreateDiscount(CreateDiscountRequest request, ServerCallContext context)
    {
        return base.CreateDiscount(request, context);
    }

    public override Task<CouponModel> UpdateDiscount(UpdateDiscountRequest request, ServerCallContext context)
    {
        return base.UpdateDiscount(request, context);
    }

    public override Task<DeleteDiscountResponse> DeleteDiscount(DeleteDiscountRequest request, ServerCallContext context)
    {
        return base.DeleteDiscount(request, context);
    }
}
```

- Oluşturduğumuz sınıfın, .proto dosyasının oluşturulmasının ardından alınan build ile Generated olarak oluşan DiscountProtoService sınıfından kalıtım almasını sağlıyoruz. Bu sayede override'ları uygulayabileceğiz.
- Override'lar, kod içerisnde tanımlanmıştır. Normal CRUD operasyonlarından farkı yok

### DiscountContext sınıfının oluşturulması

```
public class DiscountContext : DbContext
{
    public DbSet<Coupon> Coupons { get; set; } = default!;

    public DiscountContext(DbContextOptions<DiscountContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Coupon>().HasData(
            new Coupon { Id = 1, ProductName = "IPhone 6", Description = "IPhone description", Amount = 10 },
            new Coupon { Id = 2, ProductName = "Samsung 10", Description = "Samsung description", Amount = 20 }
            );
    }
}
```
- EF Core ile standart DB oluşturma işleminden hiçbir farkı yok.
- Fakat yeni bir durum var ki, biz Docker ortamında çalışıyoruz. Yani Package-Manager Console'dan *Add-Migration* ve *Update-Database* komutlarını çağırma şansımız yok. Bu sebepten ötürü, migration işlemini otomatize edeceğiz.
- Migration işlemini bir metotlar bütünü olarak, extension metotta tanımladık;

    ```
    public static class Extensions
    {
        public static IApplicationBuilder UseMigration(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<DiscountContext>();
            dbContext.Database.MigrateAsync();

            return app;
        }
    }
    ```
    - *CreateScope* servislerin bütününü teşkil eden ApplicationServices'tan bir scope ürettik. Disposable özelliği gösteren bu scope üzerinden, DB servisine erişeceğiz
    - DiscountContext'ten bir örneği, *GetRequiredService* ile alarak using keyword ile yine bir Disposable özelliğini kullandık
    - *MigrateAsync* metodu ile migration işlemini tamamlamış olduk

### Docker Operasyonları

> önceki servislerde olduğu gibi aynı işlemleri yürütüyoruz, sadece SQLite kullandığımız için connection string daha farklı. DB dosyası dahili olduğu için container'a almadık

1. Projemize sağ tıklayıp Add>Docker Support diyoruz ve varolan Dockerfile'ı override ediyoruz.

2. Projeye sağ tıklayıp Add>Container Orchestratator Support diyoruz ve varolan docker-compose.yml ile docker-compose.override.yml dosyalarımız bu sayede güncelleniyor. 

3. docker-compose.override.yml dosyasında şu değişiklik ve eklemelerde bulunuyoruz; 
    - Projede kullanacağımız port numaraları (ports field'ı)
    - Connection string (Server=discountdb)

4. docker-compose projesini ayağa kaldırıyoruz

---

## Basket Servisi Tarafında gRPC Tüketiminin Sağlanması

- Basket servisi, Discount servisini tüketmek için bir Client davranışı sergilemelidir.

### Servislerin Bağlanması
1. Basket.API projesinde bulunan _Connected Services_ kısmına gelip _Manage Services_'ı seçiyoruz. 
1. File olarak Discount servisinde oluşturmuş olduğumuz ve Server görevi görev .proto dosyasını, type olarak da Client davranışı göstermek istediğimiz için Client'ı seçiyoruz. 
1. Bu işlem sonucu Grpc kütüphanesine referans vermiş oluyoruz. Ayrıca dahil etmiş olduğumuz .proto dosyasını tüketecek şekilde Client için olan .proto dosyası otomatik olarak oluşuyor. 
1. Build aldıktan sonra Discount servisindeki .proto dosyasında olduğu şekilde, bu serviste de aynı konumda Generated classları görebiliriz.

### gRPC Tüketimi
- Tüketim işleminin uygulanacağı sınıf olan _StoreBasketHandler_'ı refactor etmeye ihtiyacımız var.
```
public class StoreBasketCommandHandler
    (IBasketRepository repository, DiscountProtoService.DiscountProtoServiceClient discountProto)
    : ICommandHandler<StoreBasketCommand, StoreBasketResult>
{
    public async Task<StoreBasketResult> Handle(StoreBasketCommand command, CancellationToken cancellationToken)
    {
        await DeductDiscount(command.Cart, cancellationToken);

        await repository.StoreBasket(command.Cart, cancellationToken);

        return new StoreBasketResult(command.Cart.UserName);
    }

    private async Task DeductDiscount(ShoppingCart cart, CancellationToken cancellationToken)
    {
        foreach (var item in cart.Items)
        {
            var coupon = await discountProto.GetDiscountAsync(new GetDiscountRequest { ProductName = item.ProductName }, cancellationToken: cancellationToken);
            item.Price -= coupon.Amount;
        }
    }
}
```
</br></br>
1. Constructor'a, build aldıktan sonra Generated olarak oluşan DiscountProtoService'da bulunan DiscountProtoServiceClient sınıfını inject ediyoruz.
1. Server'da override ettiğimiz metotları bu şekilde direkt olarak kullanabiliyoruz. Get metodunu kendimiz override etmiştik, burada ekstra bir kod tekrarına gitmedik yazdığımız kodu direkt kullanabildik Client olduğumuz için

### Konfigürasyonların yapılması
   1. Bu yaptığımız tüketim işlemi tek başına yeterli değil, bu yüzden birtakım yapılandırmalara ihtiyaç duyuyoruz. Proto servisin client olarak görev alabilmesi adına Program.cs'te şu tanımı yapıyoruz;
        ```
        builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(options =>
        {
            options.Address = new Uri(builder.Configuration["GrpcSettings:DiscountUrl"]!);
        });
        ```
   1. json dosyasında da ilgili alanı oluşturuyoruz
    ```
      "GrpcSettings": {
        "DiscountUrl":  "https://localhost:5052"
      },
    ```
 
   > henüz bilmemekle beraber tahminim; Server görevi görevek servislerde AddGrpc, Client görevi görecek servislerde ise AddGrpcClient metotlarından faydalanıyoruz


### Docker Operasyonları
- Yaptığımız işleri Docker ortamına da yansıtmamız gerekiyor. Çünkü yeni bir tüketim operasyonu ekledik ve appsettings.json dosyasından eriştiğimiz bağlantıyı Docker'a eklemedik. Basket.API'da yeni bir Dockerfile oluşturmak bir şeyi değiştirmiyor, docker-compose projesini değiştirmemiz gerekiyor.

1. docker-compose.override.yml dosyasında, basket.api'ın environment field'ında gRPC bağlantısını temsil etmesi için şu alanı oluşturuyoruz; (burada container adı yer almalı, https port kullandığımız için de 8081'i seçtik.)

```
      - GrpcSettings__DiscountUrl=https://discount.grpc:8081
```

2. Yine basket.api'de depends_on field'ına discount.grpc container'ını ekliyoruz. Çünkü bu container'ın ayakta durması, artık Discount servisine de bağlı;
```
      - discount.grpc
```

3. docker-compose projesini Startup olarak ayağa kaldırıyoruz 

---

## Ordering Service

- Bu servis, Clean Architecture ve Domain-Driven Design yaklaşımlarını temel alacaktır. 
- Kullanacağımız yapıda Domain, Application, Infrastructure ve Presentation (API) katmanları yer alacaktır. 
- Operasyonlar CQRS pattern üzerinden yürütülecektir. Veri alışverişi ise EF Core üzerinden MSSQL Server'dan yapılacaktır
- Order, bir **Aggregate Root** olarak ana entity'i teşkil edecektir. Order'a bağlı olarak **Product** ve **Customer** Aggregate'leri de hizmet verecektir.
- RabbitMQ aracılığıyla event yönetimi sağlanacaktır.

### Base Class ve Interface'lerin Oluşturulması

1. Tüm katmanları oluşturduktan sonra, proje referanslarını ayarlıyoruz. Domain -> Application -> Infrastructure -> Presentation akışı izleneceği için proje referanslarını buna göre ayarladık. Presentation katmanı, ayrıca Infrastructure referansına da sahip.
1. Domain hariç gerekli katmanlara DependencyInjection adında birer sınıf oluşturduk. Bu sınıf extension metotlar içerecek olup, her katman için IoC işlemlerini ve gerekli konfigürasyonları gerektiği şekilde yapmayı sağlayacak

1. Entity'ler üzerinde soyutlama sağlamak için **IEntity** interface'ini oluşturduk;

    ```
    public interface IEntity<T> : IEntity
    {
        public T Id { get; set; }
    }

    public interface IEntity
    {
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? LastModified { get; set; }
        public string? LastModifiedBy { get; set; }
    }
    ```
    - Bu interface sayesinde her entity için geçerli olacak olan property'leri verdik. 
    - Ayrıca generic tür tanımlayarak Entity tanımında belirteceğimiz tür neyse ona göre Id almasını sağladık.

1. Oluşturduğumuz *IEntity* interface'ini implement edecek, temel bir **Entity** class'ını oluşturduk. Bu sınıfın abstract olmasına dikkat ediyoruz ki, instance'ı oluşturulamasın

1. Aggregate'ler üzerinde meydana gelen olayları temsil etmesi için, IDomainEvent interface'ini oluşturduk. Id, ne zaman gerçekleştiği ve event türünü parametre olarak tutuyor. Ayrıca bu interface'e, MediatR tarafından sağlanan *INotification* interface'ini implemente ettik. Bu sayede event dispatching işleminde MediatR'ın metotlarından faydalanabileceğiz. MediatR, Domain katmanına kurulan tek paket olarak kalacak.

    ```
    public interface IDomainEvent : INotification
    {
        Guid EventId => Guid.NewGuid();
        public DateTime OccuredOn => DateTime.Now;
        public string EventType => GetType().AssemblyQualifiedName;
    }
    ```

1. Aggregate'leri temsil etmesi için generic ve non-generic olmak üzere iki adet interface tanımladık. Her aggregate için geçerli olmasını şart koştuğumuz property ve metotları bu interface'e geçtik. Ardından oluşturduğumuz generic interface'in implementasyonunu oluşturduk. DomainEvent'ları _domainEvents field'ında private olarak tuttuk, bu field'a erişim için ise interface'ten implemente ettiğimiz *DomainEvents* property'sinden faydalandık. *ClearDomainEvents* metodu ile de kuyruktaki tüm event'ları temizleyip bu event'ları bir array halinde döndürdük.

    ```
    public interface IAggregate<T> : IAggregate, IEntity<T>
    {
    }

    public interface IAggregate
    {
        IReadOnlyList<IDomainEvent> DomainEvents { get; }
        IDomainEvent[] ClearDomainEvents();
    }
    ```    

    ```
    public abstract class Aggregate<TId> : Entity<TId>, IAggregate<TId>
    {
        private readonly List<IDomainEvent> _domainEvents = [];
        public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void AddDomainEvents(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public IDomainEvent[] ClearDomainEvents()
        {
            IDomainEvent[] dequeuedEvents = _domainEvents.ToArray();

            _domainEvents.Clear();

            return dequeuedEvents;
        }
    }
    ```

### Order Aggregate'in Oluşturulması

1. İlk Aggregate Root'umuz olan Order'ı oluşturduk. OrderItem'larını, DomainEvent'lerde uyguladığımız yapıya benzer bir şekilde 2 member kullanarak yönettik. Value Object'lerimiz Address ve Payment, enumeration'ımız OrderStatus, otomatik getirdiğimiz property'miz ise TotalPrice olarak karşımıza çıktı.

    ```
    public class Order : Aggregate<Guid>
    {
        private readonly List<OrderItem> _orderItems = [];
        public IReadOnlyList<OrderItem> OrderItems => _orderItems.AsReadOnly();

        public Guid CustomerId { get; private set; } = default!;
        public string OrderName { get; private set; } = default!;
        public Address ShippingAddress { get; private set; } = default!;
        public Address BillingAddress { get; private set; } = default!;
        public Payment Payment { get; private set; } = default!;
        public OrderStatus Status { get; private set; } = OrderStatus.Pending;
        public decimal TotalPrice => OrderItems.Sum(x => x.Price * x.Quantity);
    }
    ```

1. Order'a bağımlı olan entity'lerimiz OrderItem, Customer ve Product'u (buraya değineceğiz şimdilik geçiyoruz) oluşturduk.
1. Order'a bağımlı olan value-object'lerimiz Address ve Payment'ı oluşturduk.
1. Order'a bağımlı olan enumeration'umuz OrderStatus'ü oluşturduk.

### Strongly-Typed ID kullanımı 

- OrderItem class'ına dikkat edersek, çok sayıda aynı type'a sahip ID kullanılmıştır. Bu durum uzun vadede karşımıza **Primitive Obsession** sorununu ortaya çıkartacaktır. 

  > Primitive Obsession: primitive değerlerin direkt olarak kullanımının oluşturabileceği karmaşıklık ve hata potansiyelidir. Örneğin; orderId, customerId, productId parametrelerinin hepsi için Guid kullanmak bu ID'leri karıştırmamıza zemin hazırlar.
- Bu sorunu çözmek için **Strongly-Typed ID Pattern** uygulayacağız. OrderId, CustomerId ve ProductId gibi değerler, her biri birer type olarak tanımlanacak ve bu type'lardaki *Value* property'si ile de Id'lerin kendisine erişeceğiz.
- Buna binaen entity'ler için birer strongly-typed ID tanımladık ve bu ID'leri generic yapıda da Guid yerine seçtik ki, her entity kendine özgü ID'si ile işaretlensin

### Enriching Entities and Value-Objects

- Entityler için *Create* metodu, value-object'lar için de *Of* metodu implemente edildi. Bu sayede bu type'ların new'lenememesi sağlandı ve daha zengin bir type iç-yönetimi sağlandı. Artık nesnelerimiz statik metotlar yardımıyla instance üretebiliyor
- Order metodu bir Aggregate Root olduğu için daha çeşitli metotlar elde etti. OrderItem eklemesi ve Order güncellemesi gibi işlemler için statik olmayan metotlar tanımlandı

### Domain Event'ların eklenmesi

- Bu event'ları *asynchronous event*'larla karıştırmıyoruz. Buradaki mantık tamamen senkron bir iletişime, aggregate'lerin iç iletişimine dayanıyor.
- Başlangıç olarak OrderCreatedEvent ve OrderUpdatedEvent class'larımızı tanımladık.