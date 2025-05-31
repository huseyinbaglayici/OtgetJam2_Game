public interface IVacuumable
{
    bool CanBeVacuumed { get; }
    float VacuumResistance { get; } // 0-1 arası (0 = direnç yok, 1 = tam direnç)
    void OnVacuumStart();
    void OnVacuumEnd();
    void OnVacuumPull(UnityEngine.Vector3 direction, float force);
}