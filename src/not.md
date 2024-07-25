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

1. Bu serviste Vertical Slice Architecture kullanacağız. Bu mimari hem makro hem de mikro düzeyde uygulanabilir. Features adında bir klasör tanımlayıp ardından her modele özgü klasör açıp bu klasörde operasyonları tanımlayarak bu mimariyi sağlayabiliriz. Bu servis özelinde "Products" klasörü bunu sağlamamızda etkili olacaktır.

1. İlk handler sınıfımız olan ```CreateProductHandler```'da ek olarak bu iş sınıfının modelini de record olarak tanımlıyoruz. Klasik CQRS pattern'ü uygulayacağız.

1. BuildingBlocks adı verilen bir class library tanımladık. Burada tüm projemize etkiyecek soyutlamaları, konfigürasyonları ve diğer bazı genellemeleri uygulayacağız. (Common project)

1. ICommand soyutlamamızda kullandığımız ```public interface ICommand : IRequest<Unit>``` tanımlamamız, bize void dönüşlü metotları bu sınıftan kalıtım aldıktan sonra kullanabilmemize olanak tanıyacak. Unit, CQRS'e ait özel bir sınıftır ve void tipi ifade eder.