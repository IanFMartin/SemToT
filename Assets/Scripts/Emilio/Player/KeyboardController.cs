using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardController : IController
{
    PlayerModel _playerModel;
    PlayerView _playerView;   

    public KeyboardController(PlayerModel m, PlayerView v)
    {
        _playerModel = m;
        _playerView = v;

        _playerModel.OnMove += _playerView.OnMove;
        _playerModel.OnAttack += _playerView.OnAttack;
        _playerModel.OnRangeAttack += _playerView.OnRangeAttack;
        _playerModel.OnSpecial += _playerView.OnSpecial;
        _playerModel.OnDash += _playerView.OnDash;
        _playerModel.OnJump += _playerView.OnJump;
        _playerModel.OnIdle += _playerView.OnIdle;
        _playerModel.OnHit += _playerView.OnHit;
        _playerModel.OnDeath += _playerView.OnDeath;
    }

    public void OnFixedUpdate()
    {
        _playerModel.Move(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (_playerModel.dashInput)
            _playerModel.Dash();

        if (_playerModel.jumpInput)
            _playerModel.Jump();
    }

    public void OnUpdate()
    {
        if (Input.GetMouseButtonDown(0))
            _playerModel.Attack();

        if (Input.GetMouseButtonDown(1))
            _playerModel.StartSpecial();
        if (Input.GetMouseButtonUp(1))
            _playerModel.EndSpecial();

        if (Input.GetKeyDown(KeyCode.LeftShift))
            _playerModel.dashInput = true;

        if (Input.GetKeyDown(KeyCode.Space))
            _playerModel.jumpInput = true;

        if (Input.GetKeyDown(KeyCode.Q))
            _playerModel.SwitchWeapon();
    }
}
