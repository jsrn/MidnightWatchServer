using System;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Mobiles;
using Server.Engines.Quests;

namespace Server.Items
{
	public class StartingSkillScroll : MidnightWatchSkillScroll
	{
		public override int LabelNumber { get { return 1094934; } } // Scroll of Transcendence
		
		public override int Message { get { return 1094933; } } /*Using a Scroll of Transcendence for a given skill will permanently increase your current 
																*level in that skill by the amount of points displayed on the scroll.
																*As you may not gain skills beyond your maximum skill cap, any excess points will be lost.*/
																
		public override string DefaultTitle { get { return String.Format( "<basefont color=#FFFFFF>Scroll of Transcendence ({0} Skill):</basefont>", Value ); } }

		public static StartingSkillScroll CreateRandom( int min, int max )
		{
			return new StartingSkillScroll(Utility.RandomMinMax(min, max) * 0.1);
		}

		[Constructable]
		public StartingSkillScroll() : this( 0.0 )
		{
		}
		
		[Constructable]
		public StartingSkillScroll( double value ) : base( value )
		{
			ItemID = 0x14EF;
			if (value == 0.0)
			{
				Name = "Skill Scroll";
			}
			else
			{
				Name = "Skill Scroll [" + value + ".0]";
			}
			Movable = false;
			LootType = LootType.Blessed;
		}

		public StartingSkillScroll(Serial serial) : base(serial)
		{
		}

		public override bool CanUse( Mobile from )
		{
			if ( !base.CanUse( from ) )
				return false;
			
			PlayerMobile pm = from as PlayerMobile;
			
			if ( pm == null )
				return false;

			return true;
		}

		public override void Use( Mobile from, int skillIndex )
		{
			if ( !CanUse( from ) )
				return;
			
			double tskill = from.Skills[skillIndex].Base; // value of skill without item bonuses etc
			double tcap = from.Skills[skillIndex].Cap; // maximum value permitted
			bool canGain = false;
			
			double newValue = Value;

			if ( newValue != 0.0 && tskill != 0.0 )
			{
				return; // Don't let people use the starting scrolls multiple times on one skill.
			}

			if ( newValue == 0.0 ) {
				if ( tskill < 50 )
				{
					newValue = 10.0;
				}
				else if ( tskill < 70 )
				{
					newValue = 5.0;
				}
				else if ( tskill < 90 )
				{
					newValue = 2.0;
				}
				else {
					newValue = 1.0;
				}
			}

			if ( ( tskill + newValue ) > tcap )
				newValue = tcap - tskill;

			if ( tskill < tcap && from.Skills[skillIndex].Lock == SkillLock.Up )
			{
				if ( ( from.SkillsTotal + newValue * 10 ) > from.SkillsCap )
				{
					int ns = from.Skills.Length; // number of items in from.Skills[]

					for ( int i = 0; i < ns; i++ )
					{
						// skill must point down and its value must be enough
						if ( from.Skills[i].Lock == SkillLock.Down && from.Skills[i].Base >= newValue )
						{
							from.Skills[i].Base -= newValue;
							canGain = true;
							break;
						}
					}
				}
				else
					canGain = true;
			}
			
			if ( !canGain )
			{
				from.SendLocalizedMessage( 1094935 );	/*You cannot increase this skill at this time. The skill may be locked or set to lower in your skill menu.
														*If you are at your total skill cap, you must use a Powerscroll to increase your current skill cap.*/
				return;
			}

			from.SendMessage("Hard work pays off. You feel a little more skillful.");
					
			from.Skills[skillIndex].Base += newValue;

			if ( from.Skills[skillIndex].Base > 100.0 )
			{
				from.Skills[skillIndex].Base = 100.0;
			}

			Effects.PlaySound( from.Location, from.Map, 0x1F7 );
			Effects.SendTargetParticles( from, 0x373A, 35, 45, 0x00, 0x00, 9502, (EffectLayer)255, 0x100 );
			Effects.SendTargetParticles( from, 0x376A, 35, 45, 0x00, 0x00, 9502, (EffectLayer)255, 0x100 );

			Delete();
		}
		
		public override void Serialize( GenericWriter writer )
		{
			base.Serialize(writer);

			writer.Write((int)0); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize(reader);

			int version = ( InheritsItem ? 0 : reader.ReadInt() ); //Required for MidnightWatchSkillScroll insertion
			LootType = LootType.Blessed;
		}
	}
}
