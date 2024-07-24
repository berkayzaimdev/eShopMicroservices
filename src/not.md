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