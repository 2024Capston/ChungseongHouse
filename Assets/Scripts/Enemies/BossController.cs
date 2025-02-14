using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class BossController : NetworkBehaviour, IShootable
{
    [SerializeField] private HitZone[] _hitZones;
    [SerializeField] Scrollbar _healthBar;

    private float _maxHealth = 10f;
    private float _health = 10f;
    private bool _inRoutine = false;

    public bool OnShot(BulletController bullet)
    {
        if (IsServer)
        {
            _health -= 1f;
            UpdateHealthBarClientRpc(_health);
            return true;
        }
        else
        {
            return false;
        }
    }

    void Update()
    {
        if (!IsServer)
        {
            return;
        }

        if (_health <= 0f)
        {
            foreach(HitZone hitZone in _hitZones)
            {
                hitZone.DisableHitZoneClientRpc();
            }

            OnDeathClientRpc();
        }

        if (!_inRoutine)
        {
            _inRoutine = true;
            StartCoroutine("PatternCoroutine");
        }
    }

    [ClientRpc]
    public void OnDeathClientRpc()
    {
        _healthBar.gameObject.SetActive(false);
        Destroy(gameObject);
    }

    [ClientRpc]
    public void UpdateHealthBarClientRpc(float newHealth)
    {
        _healthBar.size = newHealth / _maxHealth;
    }

    IEnumerator PatternCoroutine()
    {
        int stage = 0;

        while (true)
        {
            if (_health < 0f)
            {
                break;
            }

            if (stage == 20)
            {
                _hitZones[0].EnableHitZoneClientRpc();
                _hitZones[1].EnableHitZoneClientRpc();
            }
            else if (stage == 45)
            {
                _hitZones[0].DoHitDamageClientRpc();
                _hitZones[1].DoHitDamageClientRpc();
            }
            else if (stage == 50)
            {
                _hitZones[0].DisableHitZoneClientRpc();
                _hitZones[1].DisableHitZoneClientRpc();
            }
            else if (stage == 70)
            {
                _hitZones[2].EnableHitZoneClientRpc();
            }
            else if (stage == 95)
            {
                _hitZones[2].DoHitDamageClientRpc();
            }
            else if (stage == 100) {
                _hitZones[2].DisableHitZoneClientRpc();
                break;
            }

            yield return new WaitForSeconds(0.1f);
            stage++;
        }

        _inRoutine = false;
    }
}
