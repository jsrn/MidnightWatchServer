using System;
using System.Collections.Generic;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

namespace Server.Items {
	public class MedKit : Item {
		public static int Range = 2; 

		public override double DefaultWeight {
			get { return 5; }
		}

		[Constructable]
		public MedKit() : base(0xF9D) {
			Hue = 0x24;
			Name = "a first aid kit";
		}

		public MedKit(Serial serial) : base(serial) { }

		public override void Serialize(GenericWriter writer) {
			base.Serialize(writer);

			writer.Write((int) 0); // version
		}

		public override void Deserialize(GenericReader reader) {
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}

		public override void OnDoubleClick(Mobile from) {
			if (from.InRange(GetWorldLocation(), Range)) {
				from.RevealingAction();
				from.SendMessage("Who will you use the medkit on?");
				from.Target = new InternalTarget( this );
			} else {
				from.SendLocalizedMessage( 500295 ); // You are too far away to do that.
			}
		}

		private class InternalTarget : Target
		{
			private MedKit m_MedKit;

			public InternalTarget( MedKit MedKit ) : base( MedKit.Range, false, TargetFlags.Beneficial )
			{
				m_MedKit = MedKit;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				if ( m_MedKit.Deleted )
					return;

				if ( targeted is PlayerMobile && ((PlayerMobile)targeted).Undead )
				{
					if( from == targeted )
						from.SendMessage( "You have no need for the healing techniques of mortals." );
					else
						from.SendLocalizedMessage( 500970 ); // MedKits can not be used on that.
					return;
				}

				if ( targeted is Mobile && targeted is PlayerMobile )
				{
					if ( from.InRange( m_MedKit.GetWorldLocation(), MedKit.Range ) )
					{
						if ( MedKitContext.BeginHeal( from, (Mobile)targeted ) != null )
						{
							m_MedKit.Consume();
						}
					}
					else
					{
						from.SendLocalizedMessage( 500295 ); // You are too far away to do that.
					}
				}
				else
				{
					from.SendLocalizedMessage( 500970 ); // MedKits can not be used on that.
				}
			}
		}
	}

	public class MedKitContext
	{
		private Mobile m_Healer;
		private Mobile m_Patient;
		private int m_Slips;
		private Timer m_Timer;

		public Mobile Healer{ get{ return m_Healer; } }
		public Mobile Patient{ get{ return m_Patient; } }
		public int Slips{ get{ return m_Slips; } set{ m_Slips = value; } }
		public Timer Timer{ get{ return m_Timer; } }

		public void Slip()
		{
			m_Healer.SendLocalizedMessage( 500961 ); // Your fingers slip!
			++m_Slips;
		}

		public MedKitContext( Mobile healer, Mobile patient, TimeSpan delay )
		{
			m_Healer = healer;
			m_Patient = patient;

			m_Timer = new InternalTimer( this, delay );
			m_Timer.Start();
		}

		public void StopHeal()
		{
			m_Table.Remove( m_Healer );

			if ( m_Timer != null )
				m_Timer.Stop();

			m_Timer = null;
		}

		private static Dictionary<Mobile, MedKitContext> m_Table = new Dictionary<Mobile, MedKitContext>();

		public static MedKitContext GetContext( Mobile healer )
		{
			MedKitContext bc = null;
			m_Table.TryGetValue( healer, out bc );
			return bc;
		}

		public void EndHeal()
		{
			StopHeal();

			int healerNumber = -1, patientNumber = -1;
			bool playSound = true;
			bool checkSkills = false;

			if ( !m_Healer.Alive )
			{
				healerNumber = 500962; // You were unable to finish your work before you died.
				patientNumber = -1;
				playSound = false;
			}
			else if ( !m_Healer.InRange( m_Patient, MedKit.Range ) )
			{
				healerNumber = 500963; // You did not stay close enough to heal your target.
				patientNumber = -1;
				playSound = false;
			}
			else if ( !m_Patient.Alive )
			{
				m_Healer.SendMessage("They collapsed before you could finish your work.");
				playSound = false;
			}
			else if ( MortalStrike.IsWounded( m_Patient ) || m_Patient.Poisoned || BleedAttack.IsBleeding( m_Patient ) )
			{
				m_Healer.SendMessage("You're going to need to ask them to sit still!");
				playSound = false;
			}
			else
			{
				SkillName primarySkill = SkillName.Healing;
				SkillName secondarySkill = SkillName.Anatomy;

				checkSkills = true;

				double healing = m_Healer.Skills[primarySkill].Value;
				double anatomy = m_Healer.Skills[secondarySkill].Value;
				double chance = ((healing + 10.0) / 100.0) - (m_Slips * 0.02);

				if ( chance > Utility.RandomDouble() )
				{
					((PlayerMobile)m_Patient).InjuryPoints -= 5;
					m_Patient.SendMessage("They finish patching you up.");
					m_Healer.SendMessage("You finish treating the patient.");
				}
				else
				{
					m_Healer.SendMessage("You have a go at them, but seem to make things worse.");
					((PlayerMobile)m_Patient).InjuryPoints += 1;
					playSound = false;
				}
			}

			if ( healerNumber != -1 )
				m_Healer.SendLocalizedMessage( healerNumber );

			if ( patientNumber != -1 )
				m_Patient.SendLocalizedMessage( patientNumber );

			if ( playSound )
				m_Patient.PlaySound( 0x57 );
		}

		private class InternalTimer : Timer
		{
			private MedKitContext m_Context;

			public InternalTimer( MedKitContext context, TimeSpan delay ) : base( delay )
			{
				m_Context = context;
				Priority = TimerPriority.FiftyMS;
			}

			protected override void OnTick()
			{
				m_Context.EndHeal();
			}
		}

		public static MedKitContext BeginHeal( Mobile healer, Mobile patient )
		{
			bool isDeadPet = ( patient is BaseCreature && ((BaseCreature)patient).IsDeadPet );

			if ( patient is Golem )
			{
				healer.SendLocalizedMessage( 500970 ); // MedKits cannot be used on that.
			}
			else if ( patient is BaseCreature && ((BaseCreature)patient).IsAnimatedDead )
			{
				healer.SendLocalizedMessage( 500951 ); // You cannot heal that.
			}
			else if ( patient.Poisoned || BleedAttack.IsBleeding( patient ) )
			{
				healer.SendMessage("You cannot do that right now.");
			}
			else if (!patient.Alive)
			{
				healer.SendMessage("It's too late for them.");
			}
			else if (healer.CanBeBeneficial(patient, true, true))
			{
				healer.DoBeneficial( patient );

				bool onSelf = ( healer == patient );
				int dex = healer.Dex;

				double seconds = 10;
				double resDelay = ( patient.Alive ? 0.0 : 5.0 );

				MedKitContext context = GetContext( healer );

				if ( context != null )
					context.StopHeal();

				seconds *= 1000;
				
				context = new MedKitContext( healer, patient, TimeSpan.FromMilliseconds( seconds ) );

				m_Table[healer] = context;

				if ( !onSelf )
					patient.SendLocalizedMessage( 1008078, false, healer.Name ); //  : Attempting to heal you.

				
				healer.SendLocalizedMessage( 500956 ); // You begin applying the MedKits.
				return context;
			}

			return null;
		}
	}
}
