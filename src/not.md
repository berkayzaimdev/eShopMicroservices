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

1. Portları _launchSettings.json_ dosyasında **http** ve **https** bölümlerine ait **applicationUrl** kısımlarını değiştirerek ayarladık.

1. Bu serviste **Vertical Slice Architecture** kullanacağız. Bu mimari hem makro hem de mikro düzeyde uygulanabilir. Features adında bir klasör tanımlayıp ardından her modele özgü klasör açıp bu klasörde operasyonları tanımlayarak bu mimariyi sağlayabiliriz. Bu servis özelinde "Products" klasörü bunu sağlamamızda etkili olacak.

1. İlk handler sınıfımız olan ```CreateProductHandler```'da ek olarak bu iş sınıfının modelini de record olarak tanımlıyoruz. Klasik **CQRS pattern**'ü uygulayacağız.

1. **BuildingBlocks** adı verilen bir class library tanımladık. Burada tüm projemize etkiyecek soyutlamaları, konfigürasyonları ve diğer bazı genellemeleri uygulayacağız. (Common project)

1. **ICommand** interface'ini oluşturduk. Burada kullandığımız ```public interface ICommand : IRequest<Unit>``` tanımlamamız, bize void dönüşlü metotları bu interface'i implement ettikten sonra kullanabilmemize olanak tanıyacak. Unit, CQRS'e ait özel bir sınıftır ve void tipi ifade eder.

1. **IQuery** interface'ini oluşturduk. Query'leri soyutladık ve **notnull** constraint'i uygulayarak null değer dönmeyi engelledik.

1. **ICommandHandler** interface'inin oluşturulması

   - ```public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse> where TCommand : ICommand<TResponse>``` </br></br> Burada CQRS'e ait bir interface olan IRequestHandler'dan kalıtım aldık. TCommand generic yapısının ancak ve ancak, kendi oluşturduğumuz ICommand arayüzüne eşit olmasını sağladık. Ayrıca notnull constraint ekleyerek  null dönememesini sağladık.

   - ```public interface ICommandHandler<in TCommand> : ICommandHandler<TCommand, Unit> where TCommand : ICommand<Unit>``` </br></br> Az evvel oluşturmuş olduğumuz **ICommandHandler** interface'inin farklı bir türünü tanımladık. Bu generic tür; geriye hiçbir şey dönmez ve **TCommand** olarak Unit dönüşlü bir **ICommand** kabul eder.

1. **IQueryHandler** interface'inin oluşturulması

   - ```public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>``` </br></br> Bir önceki adımın benzeri bir tanımlama uyguladık. TRequest parametresi olarak IQuery alan ve notnull constraint'ine sahip bir interface tanımladık.

 1. **IEntityBuilder** interface'inin oluşturulması ve implementasyonu; 
	- Burada eğitimin haricinde kendim bir şeyler kattım. Reflection kullanarak generic bir entity builder oluşturdum ve bu şekilde command'dan entity üretimini sağladım. Custom mapping gibi de düşünebiliriz.

 1. **CreateProductEndpoint** sınıfının oluşturulması; 
	- Bu serviste minimal API ile çalışacağımız için endpoint'lerden faydalanacağız.
	- .NET Endpointleri için yazılmış olan **Carter** kütüphanesini projeye dahil ettik.
	- Oluşturduğumuz CreateProductEndpoint sınıfına, ICarterModule interface'ini implement ettik.
	- Mapping işlemi için **Mapster** kütüphanesini kurduk ve Adapt metodu ile mapleme işlemini gerçekleştirip response döndürdük.
	- Handler sınıfımıza **IDocumentSession**'dan bir örnek enjekte ettik. Bu sayede Marten kütüphanesinin sunmuş olduğu özelliklere erişebildik. Store metodu ile DB'ye ekleme işlemi yaptık ve SaveChangesAsync metodu ile de DB'de yaptığımız bu yeniliği kaydettik.
	
1. Serviste yaygın bir şekilde kullanacağımız kütüphanelerin entegrasyonunu kolaylaştırmak için **GlobalUsing** adında bir sınıf tanımlayıp kütüphanelerin kullanımını globalize ettik.

1. Projemize ait bir **docker-compose** projesi oluşturduk. Bunu yapmak için projeye sağ tıklayıp Container Orchestrator Support'tan Linux'u seçip onayladık.

   1. PostgreSQL veri tabanına bağlanacağımız için docker ayarları burada kritik önem arz ediyor. (buralar tekrar edilecek)
   1. docker-compose.yaml ve docker-compose.override.yaml dosyalarını PostgreSQL bilgilerini içerecek şekilde düzenledik ve kaydettik.
   1. Proje ortamını dockerize etmek için docker-compose projesini Visual Studio'dan ayağa kaldırıyoruz. Tüm imajlar ve çevre değişkenleri otomatik olarak yükleniyor. (Çalışmadı, VS'i tekrar başlatınca düzeldi)
   1. Docker Desktop'tan container detaylarına baktığımızda artık PostgreSQL'i **catalogdb** ismi ile görebilmekteyiz. Terminalden **docker ps** komutu ile container'ın çalıştığını doğruladık.
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
   9. Postman üzerinden istek atarız ve başarılı sonucu alırız. *5432 portu daha önce kullanımda olduğu için sıkça hata alındı. Sonuç olarak düzeltildi* 
   10. DB'de ```\d``` komutunu çağırarak Marten tarafından oluşturulmuş olan tablomuzu görebilir ve tablo üzerinde SQL sorguları çağırabiliriz.

  1. **GetProduct, GetProductById** operasyonları için Handler ve Endpoint sınıflarının oluşturulması