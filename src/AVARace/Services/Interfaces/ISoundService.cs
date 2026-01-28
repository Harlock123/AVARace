namespace AVARace.Services.Interfaces;

public interface ISoundService
{
    void PlayShoot();
    void PlayExplosion();
    void PlayThruster(bool isThrusting);
    void Initialize();
    void Dispose();
}
