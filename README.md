# First-Person-Interaction-Movement-System
First-Person Interaction & Movement System
Bu modül, FPS kamera açısına sahip bir oyuncunun dünyada gezinmesini ve objelerle arayüz (Interface) tabanlı etkileşime girmesini sağlayan çekirdek kontrolcüdür.

Özellikler:

Interface-Driven Raycaster: ITargetable arayüzü sayesinde PlayerRaycaster dünyadaki hiçbir objeyi ("Bu kapı mı?", "Kasa mı?") spesifik olarak tanımaz. Polimorfizm kullanarak tek bir Interact() metodu ile her şeyi tetikler. Bu sayede kod, yeni eklenecek objeler için "Open/Closed" prensibine %100 uyar.

Optimize Edilmiş Fizik Hareketi: MovementController içindeki yerçekimi ve Input vektörleri ayrı ayrı hesaplanıp, CharacterController'a tek bir Move çağrısı olarak gönderilir (Double-Call engellendi).

UI Güvenliği: EventSystem.IsPointerOverGameObject() kullanılarak, oyuncu bir UI menüsüne (Örn: Şifreli Kasa Tuşları) tıklarken, Raycast'in arkadaki 3D objeleri algılayıp sistemi bozması engellenmiştir.
