using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class SkeletonBoss : Enemy
{
    public WeaponTable wT;
    private SpawnWeapon SW;
    public List<Weapon> dropList = new List<Weapon>();


    public GameObject[] Special1AboveSpawnPoints;
    public GameObject[] Special1LeftSpawnPoints;
    public GameObject[] barrier;
    public GameObject prefabBullet;
    public GameObject bulletHolder;
    public GameObject prefabSlash;
    public GameObject redCircle;
    public GameObject prefabPortal;
    public ParticleSystem prefabExplosion;
    public Transform SlashHolder;
    public int radiusToDetect;
    public int radiusToPersuit;
    public int radiusToAttack;
    public int explosionDamage;
    public int speed;
    public Image healthbar;

    internal StateMachine<OnCondition> fsm;

    int[] currentPhase = new int[] { 1, 2, 3 };
    int howManySpacesBetwennSkulls;
    int howManyShoots;
    int howManyShootsInTheCircle;
    float timerSpecial1;
    float timerSpecial2;
    float timerSpecial3;
    float timerToAnySpecial;
    float timerToAttack;
    bool isAttacking;
    bool meEstasTomandoElPelo = false;
    bool _special1;
    bool _special2;
    bool _special3;
    bool CanUseSpecial;
    bool AlreadyPhase2;
    bool AlreadyPhase3;
    float timerBetweenShoots;

    void Start()
    {
        SW = new SpawnWeapon();
        var weaponTable = GameObject.Find("Weapon Table");
        wT = weaponTable.GetComponent<WeaponTable>();

        _special2 = true;
        howManySpacesBetwennSkulls = 3;
        howManyShoots = 3;
        timerBetweenShoots = 0.8f;
        howManyShootsInTheCircle = 8;
        timerToAnySpecial = 1;
        timerSpecial2 = 10;

        var idle = new State<OnCondition>("Idle");
        var die = new State<OnCondition>("Die");
        var scream = new State<OnCondition>("Scream");
        var special1 = new State<OnCondition>("Special1");
        var special2 = new State<OnCondition>("Special2");
        var special3 = new State<OnCondition>("Special3");
        var attack = new State<OnCondition>("Attack");
        var persuit = new State<OnCondition>("Persuit");
        var changePhase = new State<OnCondition>("ChangePhase");
        var show = new State<OnCondition>("Show");


        show.AddTransition(OnCondition.Idle, idle);

        idle.AddTransition(OnCondition.Persuit, persuit);
        idle.AddTransition(OnCondition.Scream, scream);
        idle.AddTransition(OnCondition.ChangePhase, changePhase);
        idle.AddTransition(OnCondition.Die, die);
        idle.AddTransition(OnCondition.Show, show);

        scream.AddTransition(OnCondition.Special1, special1);
        scream.AddTransition(OnCondition.Special2, special2);
        scream.AddTransition(OnCondition.Special3, special3);
        scream.AddTransition(OnCondition.Idle, idle);
        scream.AddTransition(OnCondition.Die, die);

        changePhase.AddTransition(OnCondition.Idle, idle);

        special1.AddTransition(OnCondition.Idle, idle);
        special1.AddTransition(OnCondition.Die, die);

        special2.AddTransition(OnCondition.Idle, idle);
        special2.AddTransition(OnCondition.Die, die);

        special3.AddTransition(OnCondition.Idle, idle);
        special3.AddTransition(OnCondition.Die, die);

        attack.AddTransition(OnCondition.Idle, idle);
        attack.AddTransition(OnCondition.Die, die);

        persuit.AddTransition(OnCondition.Idle, idle);
        persuit.AddTransition(OnCondition.Attack, attack);
        persuit.AddTransition(OnCondition.Scream, scream);
        persuit.AddTransition(OnCondition.Die, die);
        persuit.AddTransition(OnCondition.ChangePhase, changePhase);

        idle.OnEnter += () =>
        {
            anim.Play("Idle");
        };

        idle.OnUpdate += () =>
        {
            if ((Vector3.Distance(target.transform.position, transform.position) < radiusToPersuit) && timerToAnySpecial <= 0 && (timerSpecial1 <= 0 || timerSpecial2 <= 0 || timerSpecial3 <= 0))
            {
                fsm.Feed(OnCondition.Scream);
            }
            else if (Vector3.Distance(target.transform.position, transform.position) < radiusToPersuit)
            {
                fsm.Feed(OnCondition.Persuit);
            }
            if (Vector3.Distance(target.transform.position, transform.position) < radiusToDetect && !isAttacking)
            {
                transform.LookAt(target);
            }

        };
        show.OnEnter += () =>
        {
            print("no se porque no funciona");
            anim.Play("Explode");
        };
        changePhase.OnEnter += () =>
        {
            anim.Play("Explode");
            Explosion();
            StartCoroutine(TimerToIdle(1));
        };
        scream.OnEnter += () =>
        {
            anim.Play("Skill");
        };
        scream.OnUpdate += () =>
        {

            if (CanUseSpecial)
            {

                if (timerSpecial1 <= 0 && _special1)
                {
                    fsm.Feed(OnCondition.Special1);
                    _special1 = false;
                    _special3 = true;
                }
                else if (timerSpecial2 <= 0 && _special2)
                {
                    fsm.Feed(OnCondition.Special2);
                    _special2 = false;
                    _special1 = true;
                }
                else if (timerSpecial3 <= 0 && _special3)
                {
                    fsm.Feed(OnCondition.Special3);
                    _special3 = false;
                    _special2 = true;
                }
            }

        };
        scream.OnExit += () =>
         {
             timerToAnySpecial = 8;
         };

        special1.OnEnter += () =>
        {
            Special1();
            StartCoroutine(TimerToIdle(1));
        };

        special1.OnExit += () =>
        {
            timerSpecial1 = 21;
            CanUseSpecial = false;
        };

        special2.OnEnter += () =>
        {
            Special2();
            StartCoroutine(TimerToIdle(1));
        };

        special2.OnExit += () =>
        {

            CanUseSpecial = false;
            timerSpecial2 = 19;
        };

        special3.OnEnter += () =>
        {
            Special3();
            StartCoroutine(TimerToIdle(1));
        };

        special3.OnExit += () =>
        {

            CanUseSpecial = false;
            timerSpecial3 = 20;
        };

        attack.OnEnter += () =>
        {
            anim.Play("Attack");
            isAttacking = true;
        };

        attack.OnUpdate += () =>
        {
            if (timerToAttack > 0)
            {
                fsm.Feed(OnCondition.Idle);
            }

        };
        attack.OnExit += () =>
        {

            isAttacking = false;
        };

        persuit.OnEnter += () =>
         {
             anim.Play("Run");
         };
        persuit.OnUpdate += () =>
        {
            if (Vector3.Distance(target.transform.position, transform.position) < radiusToPersuit && timerToAnySpecial <= 0 && (timerSpecial1 <= 0 || timerSpecial2 <= 0 || timerSpecial3 <= 0))
            {
                fsm.Feed(OnCondition.Scream);
            }
            else if (Vector3.Distance(target.transform.position, transform.position) > radiusToPersuit)
            {
                fsm.Feed(OnCondition.Idle);
            }
            else if (Vector3.Distance(target.transform.position, transform.position) < radiusToAttack && timerToAttack <= 0)
            {
                fsm.Feed(OnCondition.Attack);
            }
            Vector3 dirToGo = new Vector3(target.transform.position.x - transform.position.x, 0, target.transform.position.z - transform.position.z);
            transform.forward = Vector3.Lerp(transform.forward, dirToGo, lerpSpeed);
            transform.position += transform.forward * speed * Time.deltaTime;
        };
        die.OnEnter += () =>
         {
             anim.Play("Death");
             GetWeapon();
         };

        fsm = new StateMachine<OnCondition>(idle);
    }

    // Update is called once per frame
    void Update()
    {
        healthbar.fillAmount = life / maxLife;

        if (life <= 0)
        {
            fsm.Feed(OnCondition.Die);
        }
        fsm.Update();

        if (!AlreadyPhase2 && life / maxLife < 0.6f)
        {
            AlreadyPhase2 = true;
            ChangePhase(2);
            fsm.Feed(OnCondition.ChangePhase);
        }
        else if (!AlreadyPhase3 && life / maxLife < 0.3f)
        {
            AlreadyPhase3 = true;
            ChangePhase(3);
            fsm.Feed(OnCondition.ChangePhase);
        }

        Timer();


        if (Input.GetKeyDown(KeyCode.L))
        {
<<<<<<< HEAD
            DestroyThis();
=======
            fsm.Feed(OnCondition.Die);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            Special3();
>>>>>>> parent of 263ccf4... charger y suicide en el bosque
        }
       
    }
    public enum OnCondition
    {
        Idle,
        Die,
        Scream,
        Special1,
        Special2,
        Special3,
        Attack,
        Persuit,
        ChangePhase,
        Show

    }

    public void Timer()
    {
        if (timerSpecial1 > 0)
        {
            timerSpecial1 -= Time.deltaTime;
        }

        if (timerSpecial2 > 0)
        {
            timerSpecial2 -= Time.deltaTime;
        }

        if (timerSpecial3 > 0)
        {
            timerSpecial3 -= Time.deltaTime;
        }

        if (timerToAnySpecial > 0)
        {
            timerToAnySpecial -= Time.deltaTime;
        }

        if (timerToAttack > 0)
        {
            timerToAttack -= Time.deltaTime;
        }
    }
    //funcion que se llama desde animator para avisar fin del ataque
    public void EndAttack()
    {
        timerToAttack = 1;
    }
    //funcion que se llama desde animator para avisar que se puede usar un poder especial
    public void SpecialEvent()
    {
        CanUseSpecial = true;
    }
    //para crear el slash, se ejecuta en el animator
    public void SlashEvent()
    {
        var slash = Instantiate(prefabSlash);
        slash.transform.position = SlashHolder.transform.position;
        slash.layer = gameObject.layer;
    }
    IEnumerator TimerToIdle(float timer)
    {
        yield return new WaitForSeconds(timer);
        fsm.Feed(OnCondition.Idle);
    }
    public void ChangePhase(int currentPhase)
    {
        if (currentPhase == 2)
        {
            howManySpacesBetwennSkulls = 2;
            howManyShoots = 5;
            howManyShootsInTheCircle = 12;
            timerBetweenShoots = 0.4f;
        }
        else if (currentPhase == 3)
        {
            howManySpacesBetwennSkulls = 1;
            howManyShoots = 7;
            howManyShootsInTheCircle = 16;
            timerBetweenShoots = 0.2f;
        }
    }

    //poder que invoca una hilera y una fila de calaveras .
    public void Special1()
    {
        var currentLeftSpawnpoints = Special1LeftSpawnPoints;
        var currentAboveSpawnpoints = Special1AboveSpawnPoints;

        //Para sacar el spawn de la bala de arriba
        for (int i = 0; i < howManySpacesBetwennSkulls; i++)
        {
            var rnd = Random.Range(0, currentAboveSpawnpoints.Length);
            //se intercambia la posicion de los objetos dentro del array
            var tempObj = currentAboveSpawnpoints[currentAboveSpawnpoints.Length - 1];
            currentAboveSpawnpoints[currentAboveSpawnpoints.Length - 1] = currentAboveSpawnpoints[rnd];
            currentAboveSpawnpoints[rnd] = tempObj;
        }
        for (int i = 0; i < currentAboveSpawnpoints.Length - howManySpacesBetwennSkulls; i++)
        {
            var bullet = Instantiate(prefabBullet, currentAboveSpawnpoints[i].transform.position, Quaternion.identity);
            bullet.gameObject.transform.Rotate(new Vector3(0, -90));
            bullet.transform.parent = bulletHolder.transform;
            bullet.layer = gameObject.layer;
        }
        //para sacar el spawn de la bala de la izquierda 
        for (int i = 0; i < howManySpacesBetwennSkulls; i++)
        {
            var rnd = Random.Range(0, currentLeftSpawnpoints.Length);
            var tempObj = currentLeftSpawnpoints[currentLeftSpawnpoints.Length - 1];
            currentLeftSpawnpoints[currentLeftSpawnpoints.Length - 1] = currentLeftSpawnpoints[rnd];
            currentLeftSpawnpoints[rnd] = tempObj;
        }

        for (int i = 0; i < currentLeftSpawnpoints.Length - howManySpacesBetwennSkulls; i++)
        {

            var bullet = Instantiate(prefabBullet, currentLeftSpawnpoints[i].transform.position, Quaternion.identity);
            bullet.gameObject.transform.Rotate(new Vector3(0, 180));
            bullet.transform.parent = bulletHolder.transform;
            bullet.layer = gameObject.layer;
        }

    }

    //poder que dispara una segudilla de calaveras
    public void Special2()
    {
        StartCoroutine(TimeBetweenShoot());
    }
    IEnumerator TimeBetweenShoot()
    {
        for (int i = 0; i < howManyShoots; i++)
        {

            var bullet = Instantiate(prefabBullet, gameObject.transform.position, Quaternion.identity);
            bullet.transform.position += new Vector3(0, 1, 0);
            bullet.layer = gameObject.layer;
            bullet.transform.parent = bulletHolder.transform;
            bullet.transform.right = -transform.forward;
            yield return new WaitForSeconds(timerBetweenShoots);
        }
    }
    //poder que tira calaveras todo alrededor
    public void Special3()
    {

        float angle = 360f / (float)howManyShootsInTheCircle;
        for (int i = 0; i < howManyShootsInTheCircle; i++)
        {
            Quaternion rotation = Quaternion.AngleAxis(i * angle, Vector3.up);
            Vector3 direction = rotation * Vector3.forward;

            Vector3 position = transform.position + (direction * 3);
            var bullet = Instantiate(prefabBullet, position, rotation);
            bullet.transform.position += new Vector3(0, 1, 0);
            bullet.layer = gameObject.layer;
            bullet.transform.parent = bulletHolder.transform;
        }

    }
    public void Explosion()
    {
        redCircle.SetActive(true);
        StartCoroutine(Explode());
    }
    IEnumerator Explode()
    {
        prefabExplosion.Play();
        int layerMask = ~(1 << 10);
        var explosionRadius = Physics.OverlapSphere(transform.position, 4, layerMask);
        yield return new WaitForSeconds(1);
        if (explosionRadius.Any(x => x.GetComponent<PlayerLife>()))
        {
            var player = explosionRadius.Where(x => x.GetComponent<PlayerLife>()).First();
            player.gameObject.GetComponent<PlayerLife>().TakeDamage(explosionDamage, false);
        }
        redCircle.SetActive(false);
    }

    public override void TakeDamage(float dmg, bool isCurseDmg)
    {
        Instantiate(damageParticle, transform.position + Vector3.up / 2, transform.rotation);
        life -= dmg;
    }

    private void GetWeapon()
    {

        foreach (var weapon in dropList)
        {

            GameObject weaponPrefab = weapon.gameObject;
            SW.Spawn(weaponPrefab, new Vector3(transform.position.x, 0, transform.position.z));
            if (!wT.weaponiDSpawn.Contains(weapon.iD))
            {
                wT.weaponiDSpawn.Add(weapon.iD);
                wT.weaponModelSpawn.Add(weapon.gameObject);
                wT.UpdateWeaponList();
            }

        }
    }
    public void SummonPortal()
    {
        foreach (var tree in barrier)
        {
            tree.SetActive(false);
        }
        prefabPortal.SetActive(true);
    }
    public void DestroyThis()
    {
        healthbar.enabled = false;
        SummonPortal();
        Destroy(gameObject);
    }
    //void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(transform.position, radiusToDetect);
    //    Gizmos.color = Color.blue;
    //    Gizmos.DrawWireSphere(transform.position, radiusToPersuit);
    //    Gizmos.color = Color.green;
    //    Gizmos.DrawWireSphere(transform.position, radiusToAttack);
    //}

}
