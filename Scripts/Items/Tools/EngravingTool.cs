using System;
using Server;
using Server.Mobiles;
using Server.Targeting;
using Server.Gumps;

namespace Server.Items
{
	public class EngravingTool : Item
	{
		private int m_UsesRemaining;

		[Constructable]
		public EngravingTool() : this( 10 )
		{
		}

		[Constructable]
		public EngravingTool(int uses) : base( 0x32F8 )
		{
			Weight = 1.0;
			Name = "an engraving tool";

			m_UsesRemaining = uses;
		}

		public EngravingTool( Serial serial ) : base( serial )
		{
		}
		
		public override void OnDoubleClick( Mobile from )
		{
			from.SendLocalizedMessage( 1072357 ); // Select an object to engrave.
			from.Target = new TargetWeapon( this );	
		}

		public override void OnSingleClick( Mobile from )
		{
			LabelTo( from, Name );
			LabelTo( from, "Uses: " + m_UsesRemaining );
		}

		public void Consume()
		{
			m_UsesRemaining -= 1;

			if ( m_UsesRemaining == 0 )
				Delete();
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version

			writer.Write( (int) m_UsesRemaining );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();

			m_UsesRemaining = reader.ReadInt();
		}
	
		public static EngravingTool Find( Mobile from )
		{
			if ( from.Backpack != null )
				return from.Backpack.FindItemByType( typeof( EngravingTool ) ) as EngravingTool;
				
			return null;
		}
		
		private class TargetWeapon : Target
		{
			private EngravingTool m_Tool;

			public TargetWeapon( EngravingTool tool ) : base( -1, true, TargetFlags.None )
			{
				m_Tool = tool;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( m_Tool == null || m_Tool.Deleted )
					return;

				Item item = (Item) targeted;

				if ( !item.IsChildOf( from.Backpack ) )
				{
					from.SendMessage("You can only engrave things that are in your backpack.");
					return;
				}

				from.CloseGump( typeof( InternalGump ) );
				from.SendGump( new InternalGump( m_Tool, item ) );
			}
		}
		
		private class InternalGump : Gump
		{
			private EngravingTool m_Tool;
			private Item m_Target;
		
			private enum Buttons
			{
				Cancel,
				Okay,
				Text
			}
		
			public InternalGump( EngravingTool tool, Item target ) : base( 0, 0 )
			{
				m_Tool = tool;
				m_Target = target;
			
				Closable = true;
				Disposable = true;
				Dragable = true;
				Resizable = false;
				
				AddBackground( 50, 50, 400, 300, 0xA28 );

				AddPage( 0 );

				AddHtmlLocalized( 50, 70, 400, 20, 1072359, 0x0, false, false ); // <CENTER>Engraving Tool</CENTER>
				AddHtmlLocalized( 75, 95, 350, 145, 1076229, 0x0, true, true ); // Please enter the text to add to the selected object. Leave the text area blank to remove any existing text.  Removing text does not use a charge.
				AddButton( 125, 300, 0x81A, 0x81B, (int) Buttons.Okay, GumpButtonType.Reply, 0 );
				AddButton( 320, 300, 0x819, 0x818, (int) Buttons.Cancel, GumpButtonType.Reply, 0 );
				AddImageTiled( 75, 245, 350, 40, 0xDB0 );
				AddImageTiled( 76, 245, 350, 2, 0x23C5 );
				AddImageTiled( 75, 245, 2, 40, 0x23C3 );
				AddImageTiled( 75, 285, 350, 2, 0x23C5 );
				AddImageTiled( 425, 245, 2, 42, 0x23C3 );
				
				AddTextEntry( 75, 245, 350, 40, 0x0, (int) Buttons.Text, "" );
			}
			
			public override void OnResponse( Server.Network.NetState state, RelayInfo info )
			{		
				if ( m_Tool == null || m_Tool.Deleted || m_Target == null || m_Target.Deleted )
					return;
			
				if ( info.ButtonID == (int) Buttons.Okay )
				{
					TextRelay relay = info.GetTextEntry( (int) Buttons.Text );
					
					if ( relay != null )
					{
						if ( String.IsNullOrEmpty( relay.Text ) )
						{
							state.Mobile.SendLocalizedMessage( 1072363 ); // The object was not engraved.
							return;
						}
						else
						{
							if( relay.Text.Length > 64 )
								m_Target.Name = relay.Text.Substring( 0, 64 );
							else
								m_Target.Name = relay.Text;
						
							state.Mobile.SendLocalizedMessage( 1072361 ); // You engraved the object.	
							m_Tool.Consume();
						}
					}
				}
				else
					state.Mobile.SendLocalizedMessage( 1072363 ); // The object was not engraved.
			}
		}
		
		public class ConfirmGump : Gump
		{						
			private EngravingTool m_Engraver;
			private Mobile m_Guildmaster;
			
			private enum Buttons
			{
				Cancel,
				Confirm
			}
		
			public ConfirmGump( EngravingTool engraver, Mobile guildmaster ) : base( 200, 200 )
			{			
				m_Engraver = engraver;
				m_Guildmaster = guildmaster;
			
				Closable = false;
				Disposable = true;
				Dragable = true;
				Resizable = false;
			
				AddPage( 0 );

				AddBackground( 0, 0, 291, 133, 0x13BE );
				AddImageTiled( 5, 5, 280, 100, 0xA40 );
				
				AddButton( 160, 107, 0xFB7, 0xFB8, (int) Buttons.Confirm, GumpButtonType.Reply, 0 );				
				AddButton( 5, 107, 0xFB1, 0xFB2, (int) Buttons.Cancel, GumpButtonType.Reply, 0 );
				AddHtmlLocalized( 40, 109, 100, 20, 1060051, 0x7FFF, false, false ); // CANCEL
			}
			
			public override void OnResponse( Server.Network.NetState state, RelayInfo info )
			{		
				if ( m_Engraver == null || m_Engraver.Deleted )
					return;
					
				//if ( info.ButtonID == (int) Buttons.Confirm )
				//	m_Engraver.Recharge( state.Mobile, m_Guildmaster );
			}
		}
	}
}	
