namespace CombatAI.API.Core
{
    public abstract class BaseCombatAIModule
    {
        public BaseCombatAI Owner { get; private set; }

        public void BaseInit(BaseCombatAI owner)
        {
            Owner = owner;
            Init();
        }

        public abstract void Destroy();
        public abstract void Init();
    }
}