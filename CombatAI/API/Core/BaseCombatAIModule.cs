namespace CombatAI.API.Core
{
    public abstract class BaseCombatAIModule
    {
        public BaseCombatAI Owner { get; }

        protected BaseCombatAIModule(BaseCombatAI owner)
        {
            Owner = owner;
        }

        public abstract void Destroy();
    }
}
