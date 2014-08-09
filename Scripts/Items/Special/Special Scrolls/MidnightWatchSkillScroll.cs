using System;
using Server;
using Server.Gumps;
using Server.Network;

namespace Server.Items
{
	public abstract class MidnightWatchSkillScroll : Item
	{
		private SkillName m_Skill;
		private double m_Value;
		
		#region Old Item Serialization Vars
		/* DO NOT USE! Only used in serialization of special scrolls that originally derived from Item */
		private bool m_InheritsItem;
		
		protected bool InheritsItem
		{ 
			get{ return m_InheritsItem; } 
		}
		#endregion
		
		public abstract int Message{ get; }
		public virtual int Title{ get { return 0; } }
		public abstract string DefaultTitle{ get; }

		public MidnightWatchSkillScroll( double value ) : base( 0x14F0 )
		{
			Weight = 1.0;
			m_Value = value;
		}

		public MidnightWatchSkillScroll( Serial serial ) : base( serial )
		{
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public SkillName Skill
		{
			get { return m_Skill; }
			set { m_Skill = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public double Value
		{
			get { return m_Value; }
			set { m_Value = value; }
		}
		
		public virtual string GetNameLocalized()
		{
			return String.Concat( "#", (1044060 + (int)m_Skill).ToString() );
		}

		public virtual string GetName()
		{			
			int index = (int)m_Skill;
			SkillInfo[] table = SkillInfo.Table;

			if ( index >= 0 && index < table.Length )
				return table[index].Name.ToLower();
			else
				return "???";
		}
		
		public virtual bool CanUse( Mobile from )
		{
			if ( Deleted )
				return false;
			
			if ( !IsChildOf( from.Backpack ) )
			{
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
				return false;
			}
			
			return true;
		}

		public virtual void Use( Mobile from, int skillIndex )
		{
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( !CanUse( from ) )
				return;
			
			from.CloseGump( typeof( MidnightWatchSkillScroll.InternalGump ) );
			from.SendGump( new InternalGump( from, this ) );
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 1 ); // version

			writer.Write( (double) m_Value );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 1:
				{
					m_Value = reader.ReadDouble();
					break;
				}
				case 0:
				{
					m_InheritsItem = true;		
					m_Value = reader.ReadDouble();
					break;
				}
			}
		}

		public class InternalGump : Gump
		{
			private Mobile m_Mobile;
			private MidnightWatchSkillScroll m_Scroll;

			public InternalGump( Mobile mobile, MidnightWatchSkillScroll scroll ) : base( 25, 50 )
			{
				m_Mobile = mobile;
				m_Scroll = scroll;

				AddPage( 0 );

				AddBackground( 25, 10, 840, 550, 5054 );

				AddHtml( 40, 20, 260, 30, "Starting Skill Scroll", true, false );

				// Add skill entries
				SkillInfo[] table = SkillInfo.Table;
				string name = "";
				int fakeIndex = 0;
				for ( int i = 0; i < 17; i++) {
					name = table[i].Name;
					fakeIndex += 1;

					if (name == "Discordance" || name == "Peacemaking")
					{
						fakeIndex = fakeIndex - 1;
						continue;
					}

					AddHtml( 40, 60 + (fakeIndex * 30), 220, 30, name, true, false );
					AddButton( 260, 65 + (fakeIndex * 30), 4005, 4007, i + 1, GumpButtonType.Reply, 0 );
				}

				fakeIndex = 0;
				for ( int i = 17; i < 34; i++) {
					name = table[i].Name;
					fakeIndex += 1;

					if (name == "Provocation" || name == "Spirit Speak")
					{
						fakeIndex = fakeIndex - 1;
						continue;
					}

					AddHtml( 310, 60 + (fakeIndex * 30), 220, 30, name, true, false );
					AddButton( 530, 65 + (fakeIndex * 30), 4005, 4007, i + 1, GumpButtonType.Reply, 0 );
				}

				fakeIndex = 0;
				for ( int i = 34; i < 49; i++) {
					name = table[i].Name;
					fakeIndex += 1;

					if (name == "Spirit Speak")
					{
						fakeIndex = fakeIndex - 1;
						continue;
					}

					AddHtml( 580, 60 + (fakeIndex * 30), 220, 30, name, true, false );
					AddButton( 800, 65 + (fakeIndex * 30), 4005, 4007, i + 1, GumpButtonType.Reply, 0 );
				}
			}

			public override void OnResponse( NetState state, RelayInfo info )
			{
				if (info.ButtonID != 0 ) {
					m_Scroll.Use( m_Mobile, info.ButtonID - 1 );
				}
			}
		}
	}
}
