using System;
using Server.Network;
using Server;
using Server.Spells;

namespace Server.Misc
{
	public class FoodDecayTimer : Timer
	{
		public static void Initialize()
		{
			new FoodDecayTimer().Start();
		}

		public FoodDecayTimer() : base( TimeSpan.FromMinutes( 20.0 ), TimeSpan.FromMinutes( 20.0 ) )
		{
			Priority = TimerPriority.OneMinute;
		}

		protected override void OnTick()
		{
			FoodDecay();			
		}

		public static void FoodDecay()
		{
			foreach ( NetState state in NetState.Instances )
			{
				HungerDecay( state.Mobile );
				ThirstDecay( state.Mobile );
			}
		}

		public static void HungerDecay( Mobile m )
		{
			if (m == null) return;

			// Set stat mod if player
			if (m.Player)
			{
				int diff = (20 - m.Hunger) / 2;
				m.RemoveStatMod("HungerStr");
				m.RemoveStatMod("HungerDex");
				m.RemoveStatMod("HungerInt");
				m.AddStatMod( new StatMod( StatType.Str, "HungerStr", -diff, TimeSpan.Zero ) );
				m.AddStatMod( new StatMod( StatType.Dex, "HungerDex", -diff, TimeSpan.Zero ) );
				m.AddStatMod( new StatMod( StatType.Int, "HungerInt", -diff, TimeSpan.Zero ) );
			}

			if ( m.Hunger >= 1 )
				m.Hunger -= 1;
		}

		public static void ThirstDecay( Mobile m )
		{
			if (m == null) return;

			// Set stat mod if player
			if (m.Player)
			{
				int diff = (20 - m.Thirst) / 2;
				m.RemoveStatMod("ThirstStr");
				m.RemoveStatMod("ThirstDex");
				m.RemoveStatMod("ThirstInt");
				m.AddStatMod( new StatMod( StatType.Str, "ThirstStr", -diff, TimeSpan.Zero ) );
				m.AddStatMod( new StatMod( StatType.Dex, "ThirstDex", -diff, TimeSpan.Zero ) );
				m.AddStatMod( new StatMod( StatType.Int, "ThirstInt", -diff, TimeSpan.Zero ) );
			}

			if ( m.Thirst >= 1 )
				m.Thirst -= 1;
		}
	}
}