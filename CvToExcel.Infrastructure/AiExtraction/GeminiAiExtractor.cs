using System.Text;
using System.Text.Json;
using CvToExcel.Application.Contracts;
using CvToExcel.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace CvToExcel.Infrastructure.AiExtraction;

public class GeminiAiExtractor(HttpClient httpClient,
 IOptions<GeminiOptions> options) : IAiExtractor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
    private static readonly JsonSerializerOptions CvDataJsonOptions = new ()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = {new EmptyStringToNullConverter()}
    };

    private const string Prompt = """
        Bu bir CV (özgeçmiş) PDF dosyasıdır. PDF içeriğini dikkatlice analiz et ve aşağıdaki JSON şemasına birebir uyan, yalnızca geçerli JSON formatında bir çıktı döndür:
    
        {
        "newCandidate": {
        "fullName": "string",
        "email": "string veya null",
        "phone": "string veya null",
        "location": "string veya null",
        "educations": [
        {
        "institution": "string",
        "department": "string veya null",
        "degree": "string veya null",
        "startDate": "YYYY-MM veya null",
        "endDate": "YYYY-MM veya current veya null"
        }
        ],
        "workExperiences": [
        {
        "companyName": "string",
        "jobTitle": "string",
        "startDate": "YYYY-MM veya null",
        "endDate": "YYYY-MM veya current veya null"
        }
        ],
        "skills": [
        {
        "name": "string",
        "skillType": "Technical | Soft | Language | Tool"
        }
        ],
        "languages": [
        {
        "name": "string",
        "proficiencyLevel": "string"
        }
        ],
        "projects": [
        {
        "title": "string",
        "technologiesUsed": "string veya null",
        "description":"string veya null"
        }
        ],
        "otherSections": [
        {
        "title": "string",
        "content": "string veya null"
        }
        ]
        },
        "table": {
        "columns":["string", "string"],
        "rows":[
        {"sütun adı": "hücre değeri"}
        ]
        }
        }
        Kurallar:
        Genel kurallar:
        CV'de açıkça bulunan bilgileri çıkar.
        CV'de bulunmayan hiçbir bilgiyi uydurma, tahmin etme veya ekleme.
        Bilgi açıkça mevcut değilse ilgili string alanını null bırak.
        Liste türündeki alanlar CV'de bulunmuyorsa null yerine boş array [] döndür.
        Yanıtın yalnızca JSON olması gerekir. JSON dışında hiçbir açıklama, markdown, yorum veya metin döndürme.
        JSON şemasındaki alan adlarını değiştirme, yeni alan ekleme veya mevcut alanları kaldırma.
        Telefon numaralarını normalize et: sadece baştaki "+" işaretini (varsa) ve rakamları tut, boşluk/parantez/tire gibi karakterleri kaldır. Örnek: "+90 532 315 41 02" → "+905323154102".
        
        Tarih kuralları:
        Tarihler mümkün olduğunda YYYY-MM formatında döndürülmelidir.
        Yalnızca yıl bilgisi varsa (ay belirtilmemişse), ay'ı uydurma — bu durumda ilgili tarih alanını null bırak.
        Yalnızca yeterli bilgi varsa tarih oluştur.
        CV'de bir iş veya eğitim deneyiminin hâlen devam ettiği açıkça anlaşılıyorsa endDate değerini "current" olarak döndür.
        Örneğin "2024 - Present", "2024 - Current", "Jan 2024 - Present", "Currently working" veya devam eden bir deneyimi açıkça belirten benzer ifadeler "current" olarak değerlendirilmelidir.
        Devam eden bir deneyimde endDate null değil, "current" olmalıdır.
        Başlangıç tarihi CV'de belirtilmiyorsa startDate null olmalıdır.
        Geçmişte tamamlanmış bir deneyimin bitiş tarihi belirtilmiyorsa endDate null olmalıdır.
        Bir tarihin gerçek değeri CV'den kesin olarak çıkarılamıyorsa tarih uydurma.
        
        Work experience:
        companyName şirket veya kurum adıdır.
        jobTitle kişinin o pozisyondaki unvanıdır.
        Aynı şirkette farklı pozisyonlar veya farklı dönemler açıkça ayrı deneyimler olarak verilmişse bunları ayrı workExperiences elemanları olarak çıkar.
        
        Education:
        CV'deki her ayrı eğitim kaydını ayrı education elemanı olarak çıkar.
        institution, department ve degree bilgilerini yalnızca CV'de mevcutsa doldur.
        
        Skills:
        CV'de listelenen her skill'i ayrı bir skills elemanı olarak çıkar.
        skillType yalnızca şu dört değerden biri olabilir:
        Technical
        Soft
        Language
        Tool
        skillType değerini skill'in anlamına göre belirle.
        Bunun dışında hiçbir skillType değeri kullanma.
        Bir becerinin birden fazla kategoriyle ilişkili olabileceği durumlarda CV'deki kullanım şekline en uygun kategoriyi seç.
        
        Languages:
        CV'de belirtilen dilleri çıkar.
        proficiencyLevel yalnızca CV'de belirtilen seviyeyi yansıtmalıdır.
        CV'de seviye belirtilmemişse null kullan.
        Yeni bir dil seviyesi uydurma.
        
        Projects:
        CV'de bulunan proje bilgilerini çıkar.
        CV'de bulunan her ayrı projeyi ayrı bir projects elemanı olarak oluştur.
        projects[].title zorunludur ve CV'de bulunan proje adını temsil etmelidir.
        projects[].technologiesUsed yalnızca CV'de ilgili proje için kullanılan teknolojiler açıkça belirtilmişse doldurulmalıdır.
        TechnologiesUsed bilgisini tahmin etme veya projede kullanılmış olabileceğini düşündüğün teknolojileri ekleme.
        CV'de bir proje için teknoloji belirtilmemişse technologiesUsed null olmalıdır.
        CV'de proje bulunmuyorsa projects değeri [] olmalıdır.
        projects[].description, CV'de o proje için açıklama varsa metni koru; yoksa null döndür.
        
        Other sections:
        CV'de educations, workExperiences, skills, languages veya projects kapsamında olmayan her ayrı CV bölümünü otherSections içinde çıkar.
        Örnekler: Volunteering, Certifications, Awards, Publications, References, Hobbies, Interests, Leadership, Courses ve benzeri bölümler.
        Önceden tanımlanmamış yeni bir bölüm adıyla karşılaşırsan da bunu otherSections içine ekle.
        Her ayrı CV başlığı/bölümü ayrı bir otherSections elemanı olmalıdır.
        Farklı bölümleri gereksiz yere birleştirme.
        title, CV'de kullanılan bölüm başlığını mümkün olduğunca korumalıdır.
        content, ilgili bölümün CV'deki metnini mümkün olduğunca korumalıdır.
        otherSections[].content alanında özetleme, yorumlama, yeniden yazma veya bilgi ekleme.
        CV'de standart alanlara uymayan birden fazla bölüm varsa her biri ayrı bir otherSections elemanı olmalıdır.
        CV'de other section bulunmuyorsa otherSections değeri [] olmalıdır.
         CV'de bir başlığın altında ayrı bir alt-başlık (örn. "Hobbies and Interests") varsa, bunu üst başlığın içeriğine gömme — ayrı bir otherSections elemanı olarak çıkar, title alanına o alt-başlığı yaz.
       
        Bilgi kaybını önleme:
        CV'deki anlamlı bilgileri mümkün olduğunca kaybetmeden çıkar.
        Bilgiyi yalnızca tanımlı JSON alanlarına uygun şekilde yapılandır.
        Bir bilgi standart alanlardan birine kesin olarak uymuyorsa ve CV'de ayrı bir bölüm olarak bulunuyorsa bu bilgiyi kaybetmek yerine otherSections içine koy.
        Ancak aynı bilgiyi hem standart bir alana hem de otherSections içine tekrar etme.
       
        Çıktı kuralları:
        Çıktı yalnızca geçerli JSON olmalıdır.
        JSON dışında hiçbir metin döndürme.
        Markdown code block kullanma.
        JSON alan adlarını tam olarak verilen isimlerle kullan.
        Null değerleri için yalnızca null kullan.
        Liste alanları için yalnızca JSON array kullan.
        Hâlen devam eden iş veya eğitim deneyimleri için endDate = "current" kullan.
        CV'de bulunmayan bilgileri kesinlikle uydurma.

         Mevcut adaylar ve tablo:
        Sana ayrıca "Mevcut adaylar (JSON)" başlığıyla, sistemde zaten kayıtlı adayların listesi verilir.
        Önce PDF'teki yeni CV'yi yukarıdaki kurallara göre newCandidate olarak çıkar.
        table oluştururken hem mevcut adayları hem yeni çıkardığın adayı birlikte kullan — table.rows içinde her aday için bir satır olmalı, sadece yeni aday değil.
        Projeler sütununda her proje için başlığı ve kullanılan teknolojileri birlikte yaz (örn. "Proje Adı (Teknoloji1, Teknoloji2)").
        Sütun isimlerini Türkçe kullan (örn. Ad Soyad, E-posta, Telefon, Konum, Eğitim, İş Deneyimi, Yetenekler, Diller, Projeler, Diğer).
        Bir adayın otherSections verisi ne kadar çeşitli olursa olsun, table içinde tek bir "Diğer" sütununda birleştir — her farklı otherSections başlığı için ayrı sütun açma.
        """;

    public async Task<CvProcessingResult> ProcessCvAsync(Stream pdfStream, string contentType,
    IReadOnlyList<CvExtractionResult> existingCandidates,
     CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        await pdfStream.CopyToAsync(memoryStream, cancellationToken);
        var base64Pdf = Convert.ToBase64String(memoryStream.ToArray());

        var request = new GeminiGenerateContentRequest
        {
            Contents = new List<GeminiContent>
            {
                new()
                {
                    Parts = new List<GeminiPart>
                    {
                        new() { Text = Prompt },
                        new() {Text = $"Mevcut adaylar (JSON) :\n{JsonSerializer.Serialize(existingCandidates, JsonOptions)}"},
                        new() { InlineData = new GeminiInlineData { MimeType = contentType, Data = base64Pdf } }
                    }
                }
            },
            GenerationConfig = new GeminiGenerationConfig()
        };

        var geminiOptions = options.Value;
        var requestUri = $"https://generativelanguage.googleapis.com/v1beta/models/{geminiOptions.Model}:generateContent";

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri);
        httpRequest.Headers.Add("x-goog-api-key", geminiOptions.ApiKey);
        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(request, JsonOptions),
            Encoding.UTF8,
            "application/json");

        using var httpResponse = await httpClient.SendAsync(httpRequest, cancellationToken);
        var responseBody = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Gemini API error {(int)httpResponse.StatusCode}: {responseBody}");
        }

        var geminiResponse = JsonSerializer.Deserialize<GeminiGenerateContentResponse>(responseBody, JsonOptions);

        var text = geminiResponse?.Candidates.FirstOrDefault()?.Content.Parts.FirstOrDefault()?.Text
        ?? throw new InvalidOperationException("Gemini API'den beklenen formatta bir cevap alınamadı.");

        return JsonSerializer.Deserialize<CvProcessingResult>(text, CvDataJsonOptions)
        ?? throw new InvalidOperationException("AI cevabı CvProcessingResult'a dönüştürülemedi.");
        
    }
}
