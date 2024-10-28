using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class BallHandler : MonoBehaviour
{
    [SerializeField] private GameObject ballPrefab; // top
    [SerializeField] private Rigidbody2D pivot; // yeniden doğduğu konum
    [SerializeField] private float respawnDelay = 1f; // kaç saniye sonra doğsun
    [SerializeField] private float detachDelayDuration = 0.15f;

    private Rigidbody2D currentBallRigidbody;
    private SpringJoint2D currentBallSpringJoint;

    private Camera mainCamera;
    private bool isDragging; // oyuncu topu sürüklüyor mu?

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        SpawnNewBall(); // ilk top
    }

    private void OnEnable()
    {
        // birden fazla yere tıklanırsa EnhancedTouchSupport 
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        // eğer top fırlatıldıysa yani sürüklenip bırakıldıysa
        if (currentBallRigidbody == null) { return; }

        // parmak dokunmatik ekrana temas ediyor mu?
        if (Touch.activeTouches.Count == 0) // dokunmatik ekrana dokunmuyorsak
        {
            // eğer ekrana dokunmuyorsak, topu fırlatacağımızı söyleyeceğiz
            if (isDragging) // oyuncu topu sürükledi mi?
            {
                LaunchBall(); // oyuncu topu sürükleyip bıraktıysa topu fırlat
            }
            isDragging = false; // sürüklemiyoruz

            return; // temas yoksa aşağıdaki kodlar çalıştırılmaz
        }

        isDragging = true; // sürükleniyor mu?
        currentBallRigidbody.isKinematic = true; // sürüklenirken yer çekimi ve yay etki etmesin

        Vector2 touchPosition = new Vector2();
        foreach (Touch touch in Touch.activeTouches)
        {
            // Birden fazla dokunma olduğu için bu noktaların değerlerini kullanarak orta noktayı bulacağız
            touchPosition += touch.screenPosition; // noktaları topladım
        }

        // noktaların toplamını dokunma sayısına böleceğiz
        touchPosition /= Touch.activeTouches.Count; // merkez değer, topun konumu

        // ScreenToWorldPoint: kameranın sahnedeki konumuna göre dönüşümü yapacak
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(touchPosition);
        // ekranın merkez noktası: (0,0)

        // birden fazla yere tıklanırsa top bu noktaların merkezinde duracak
        // topun konumunu noktaların ortasına eşitliyorum
        currentBallRigidbody.position = worldPosition;
    }

    private void SpawnNewBall()
    {
        // pivot noktasında, varsayılan rotasyonda 
        GameObject ballInstance = Instantiate(ballPrefab, pivot.position, Quaternion.identity);
        // oluşturulan nesnenin bileşenlerine erişeceğiz
        currentBallRigidbody = ballInstance.GetComponent<Rigidbody2D>();
        currentBallSpringJoint = ballInstance.GetComponent<SpringJoint2D>();

        // yay topu pivota bağladı
        currentBallSpringJoint.connectedBody = pivot;
    }

    private void LaunchBall()
    {
        // parmağımızı kaldırdığımız anda topun fiziğe tepki vermeye başlaması ve topun yay bağlantısından ayrılması.

        // oyuncu topu sürükleyip bıraktıysa top fırlatılacak
        currentBallRigidbody.isKinematic = false; // yer çekimi ve yay kuvveti etki etsin
        // top fırlatıldıktan sonra tıklanılan yere geri gelmesin
        currentBallRigidbody = null;

        // yayın topu geri çekmesi için gecikme koymalıyım
        // topu fırlatmak için çekerken gerilime ihtiyacım var
        Invoke(nameof(DetachBall), detachDelayDuration);
    }

    private void DetachBall()
    {
        // topu yaydan ayıracağım
        currentBallSpringJoint.enabled = false;
        currentBallSpringJoint = null;
        // top gittiğinde referansları null'a eşitledim

        // belirli bir süre sonra yeni top oluşsun
        Invoke(nameof(SpawnNewBall), respawnDelay);
    }
}

/*
Screen Space(Ekran alanı): Piksel cinsinden dokunma konumudur.
World Space: oyun dünyası içindeki birimler açısından konum.

Ekrana dokunuyoruz ve bize piksel cinsinden ekran alanı koordinatları veriliyor, 
ancak bu bilgiyi dünya üzerindeki bir konuma dönüştürmek istiyoruz, 
böylece parmağımızı koyduğumuz yere topumuzu hareket ettirebiliriz.
*/




