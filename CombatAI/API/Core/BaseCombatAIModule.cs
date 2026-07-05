namespace CombatAI.API.Core
{
    public abstract class BaseCombatAIModule
    {
        protected BaseCombatAIModule(BaseCombatAI owner)
        {
            Owner = owner;
        }

        public BaseCombatAI Owner { get; private set; }

        public abstract void Destroy();
    }
}