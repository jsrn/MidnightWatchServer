using System;
using System.Collections.Generic;
using Server.Network;
using Server;
using Server.Items;
using Server.Mobiles;

namespace Server.Misc
{
	public class SkillScrollTimer : Timer
	{
		public static void Initialize()
		{
			new SkillScrollTimer().Start();
		}

		public SkillScrollTimer() : base( TimeSpan.FromHours( 24.0 ), TimeSpan.FromHours( 24.0 ) )
		{
			Priority = TimerPriority.OneMinute;
		}

		protected override void OnTick()
		{
			GiveScrollToAllPlayers();
		}

		private static void GiveScrollToAllPlayers()
		{
			List<Mobile> mobs = new List<Mobile>( World.Mobiles.Values );
			
			foreach ( Mobile m in mobs )
			{
				if ( m.Player ) {
					Container pack = m.Backpack;
					if ( pack != null )
					{
						Item[] currentScrolls = pack.FindItemsByType( typeof( StartingSkillScroll ), false );
						if ( currentScrolls.Length < 16 )
						{
							pack.DropItem( new StartingSkillScroll(0.0) );
						}
					}
				}
			}
		}
	}
}