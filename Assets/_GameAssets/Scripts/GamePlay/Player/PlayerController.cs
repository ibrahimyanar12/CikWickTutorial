using UnityEngine;
using UnityEngine.InputSystem;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _orientationTransform;
     [Header("Movement Settings")]
     [SerializeField] private KeyCode _movementKey;
    [SerializeField] private float _movementSpeed;


    [Header("Jump Settings")]
    [SerializeField] private KeyCode _jumpKey;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _jumgCooldown;
    [SerializeField] private bool _canJump;
    
    [Header("Sliding Setting")]
    [SerializeField] private KeyCode _slideKey;
    [SerializeField] private float _slideMultiplier;
    
    [SerializeField] private float _slideDrag;

    [Header("Ground Check Settings")]
    [SerializeField] private float _playerHeight;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundDrag;
   //LayerMask Sadece zemin olarak işaretlediğim nesnelere çarpınca zıplmasına izin ver demektir.
   private Rigidbody _playerRigibody;

   private float _horizontalInput, _verticalInput;
   // Bunlar bizim parmaklarımız yön tuşlarımızı sağlayan çıktıyı unity üzerinden hazır kodlardan alabiliyoruz bunlar sayesinde
   // Horizon A VE D tuşlarını Vertical ise W ve s tuşlarının işlev kazanmasını sağlıyo.
   private Vector3 _movementDirection;
   // yukarıda girdiğim yatay ve dikey girdileri birleştirmek amacıyla bu kodu kullanırız.
   private bool _isSliding;
   

   private void Awake()
    {
        _playerRigibody = GetComponent<Rigidbody>();
        // Benim bu scripti üzerine attığım objenin fizik motorunu bul yani Rigibody ve benim playerRigibody nin içine yerleştir.
        _playerRigibody.freezeRotation = true;
        // Civcivin sağa sola çarpınca bir top gibi yuvarlanıp devrilmesini engeller,Dik durmasını sağlar.
    }
    private void Update()
    {
       SetInputs(); 
       SetPlayerDrag();
       LimitPlayerSpeed();
    }

    private void FixedUpdate()
    {
        SetPlayerMovement();
    }

    private void SetInputs()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");
        // Klavyeden Yön tuşlarına basıp basmadığını kontrol eder.Raw demesinin sebebi karakterin hemen durup hemen hızlanması içindir yumuşak bir geçiş yerine dijital,kesin sonuç verir.

        if (Input.GetKeyDown(_slideKey))
        {
            _isSliding = true;
            Debug.Log("Player Sliding");
        }
        else if (Input.GetKeyDown(_movementKey))
        {
            _isSliding = false;
            Debug.Log("Player Moving");
        }

        else if (Input.GetKey(_jumpKey) && _canJump && IsGrounded())
        {
            _canJump = false;
            SetPlayerJumping();
            Invoke(nameof(ResetJumping), _jumgCooldown);
        }
        // Zıplama tuşuna Space basılıyor mu Zıplama bekleme süresi cooldown doldu mu ayağın yere değiyo mu sorularını sorar karakterimize.
    }

    private void SetPlayerMovement()
    {
        _movementDirection = _orientationTransform.forward * _verticalInput + _orientationTransform.right * _horizontalInput;
        // MovemenDirection civcivin nereye gideceğini hesaplıyor
        //_OrientationTransform.forward * _verticalInput:"Karakterin baktığı yönün ilerisi ile ileri tuşunu çarp. 
        // _orientationTransform.right * _horizontalInput:"Karakterin sağı ile sağ tuşunu çarp".
        // Ve en sonda ikisini çarpıp en son toplama yapıyo bu sayede civcivin gidiceği rotayı buluyoruz.

        if (_isSliding)
        {
             _playerRigibody.AddForce(_movementDirection.normalized * _movementSpeed * _slideMultiplier, ForceMode.Force);
        }
        else
        {
            _playerRigibody.AddForce(_movementDirection.normalized * _movementSpeed , ForceMode.Force);
        }

       
        // .normalized:Eğer hem sağa hem ileri basarsan karakter normalden daha hızlı gider pisagor teorisinden dolayı.Bu komut hızı hep sabitte tutar,hile yapmayı engeller.
        //_playerRigibody.AddForce:İşte civcivi iten o görünmez el! 
        //Hesapladığın yönü,belirlediğin hızla çarpıp fizik motoruna"Bunu bu yöne doğru it!" diyorsun.
        //ForceMode.Force:Bu,itme işleminin sürekli bir kuvvet araba motoru gibi olduğunu söyler.
    }
    private void SetPlayerDrag()
    {
        if (_isSliding)
        {
            _playerRigibody.linearDamping = _slideDrag;
        }
        else
        {
            _playerRigibody.linearDamping = _groundDrag;
        }
        
    }
    private void LimitPlayerSpeed()
    {
        Vector3 flatVelocity = new Vector3(_playerRigibody.linearVelocity.x,0f,_playerRigibody.linearVelocity.z);
        if(flatVelocity.magnitude > _movementSpeed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * _movementSpeed;
            _playerRigibody.linearVelocity
            = new Vector3(limitedVelocity.x,_playerRigibody.linearVelocity.y,limitedVelocity.z);
        }
    }


    private void SetPlayerJumping()
    {
        _playerRigibody.linearVelocity = new Vector3(_playerRigibody.linearVelocity.x,0f,_playerRigibody.linearVelocity.z);
        //Bu satır aslında bir "Sıfırlama" işlemi.Civciv aşağı düşerken zıplarsa,düşüş hızı zıplama gücünü kırmasın diye dikey hızı y ekseni önce 0 yapılıyor.Böylece her zıplamada aynı yükseklikte oluyo.
        _playerRigibody.AddForce(transform.up * _jumpForce, ForceMode.Impulse);
        //Üsteki yön vermede Force.mode.Force kullanmıştık o çünkü sürekli çalışan birşeyken zıplamak sadece anlık birşeydir 
        //Bu yüzden zıplamda Force.Mode.Impulse kullanılır.
    }
    private void ResetJumping()
    {
        _canJump = true;
        //Yukarda Invoke ile bu fonksiyonu çağırdık.Tek yaptığı iş _canJump = true; yaparak civcivin tekrar zıplayabilmesini sağlamak bir nevi "dolum süresi" cooldown bitti diyor.
    }
    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, _playerHeight * 0.5f + 0.2f, _groundLayer);
        //Physic.Raycast:Civcivin merkezinden yere doğru hayali bir lazer ışını yollar
        //_playerHeight * 0.5f + 0.2f:Lazerin uzunluğunu hesaplıyo.Boyunun yarısından birazcık daha uzun bir lazer yolluyor ki tam yere değdiğinde "Evet,yerdeyim!" diyebilsin.
    }
}
