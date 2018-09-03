using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class OverlordManager : IService, IOverlordManager
    {
        public void ChangeExperience(Hero hero, int value)
        {
            hero.Experience += value;
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
            if (hero.Experience > 1000)
            {
                LevelUp(hero);
            }
        }

        private void LevelUp(Hero hero)
        {
            hero.Level++;
            hero.Experience = 0;
        }
    }
}
