using Loom.ZombieBattleground.View;

namespace Loom.ZombieBattleground {
    public interface ICardView : IView {
        CardModel Model { get; }

        void Dispose();
    }
}
