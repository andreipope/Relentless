using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class OverlordManager : IService, IOverlordManager
    {
        public void ChangeExperience(Hero hero, int value)
        {
            hero.experience += value;
            CheckLevel(hero);
        }

        public void Init()
        {
        }

        public void Dispose()
        {
        }

        public void Update()
        {
        }

        private void CheckLevel(Hero hero)
        {
            if (hero.experience > 1000)
            {
                LevelUP(hero);
            }
        }

        private void LevelUP(Hero hero)
        {
            hero.level++;
            hero.experience = 0;

            // hero.ValidateSkillLocking();
        }
    }
}
