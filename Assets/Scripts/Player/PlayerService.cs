
public class PlayerService
{
    private PlayerMovementViewModel _movementViewModel;
    private PlayerStatusViewModel _statusViewModel;

    private const float _hungerDecreasePerSecond = 0.5f;

    public PlayerService()
    {
        _movementViewModel = new PlayerMovementViewModel();
        _statusViewModel = new PlayerStatusViewModel();
    }

    public PlayerMovementViewModel GetMovementViewModel()
    {
        return _movementViewModel;
    }

    public PlayerStatusViewModel GetStatusViewModel()
    {
        return _statusViewModel;
    }

    public void UpdatePlayerState(int currentDay, float deltaTime)
    {
        if (currentDay >= 2)
        {
            float newHunger = _statusViewModel.Hunger - (_hungerDecreasePerSecond * deltaTime);

            if (newHunger < 0.0f)
            {
                newHunger = 0.0f;
            }

            _statusViewModel.Hunger = newHunger;

            ApplyHungerDebuff();
        }
    }

    private void ApplyHungerDebuff()
    {
        if (_statusViewModel.Hunger <= 50.0f)
        {
            float debuffRate = _statusViewModel.Hunger / 50.0f;

            if (debuffRate < 0.2f)
            {
                debuffRate = 0.2f;
            }

            _movementViewModel.ApplyHungerDebuff(debuffRate);
        }
        else
        {
            _movementViewModel.ApplyHungerDebuff(1.0f);
        }
    }
}

