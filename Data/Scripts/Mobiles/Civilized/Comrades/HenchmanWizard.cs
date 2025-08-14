using System; 
using System.Collections; 
using Server.Misc; 
using Server.Items; 
using Server.Gumps;
using Server.Mobiles; 
using Server.Network;
using Server.Regions;
using Server.Targeting;
using Server.Spells.Fifth;
using Server.Spells.First;
using Server.Spells.Fourth;
using Server.Spells.Necromancy;
using Server.Spells.Second;
using Server.Spells.Seventh;
using Server.Spells.Sixth;
using Server.Spells.Third;
using Server.Spells.Magical;
using Server.Spells.Shinobi;
using Server.Spells;

namespace Server.Mobiles 
{
	[CorpseName( "a henchman corpse" )] 
	public class HenchmanWizard : BaseCreature
	{
		private DateTime m_Healing;
		public DateTime Healing{ get{ return m_Healing; } set{ m_Healing = value; } }

		private DateTime m_NextMorale;
		public DateTime NextMorale{ get{ return m_NextMorale; } set{ m_NextMorale = value; } }

//		public override void OnMovement( Mobile m, Point3D oldLocation )
//		{
//			bool GoAway = HenchmanFunctions.OnMoving( m, oldLocation, this, m_NextMorale );
//			if ( GoAway == true ){ Timer.DelayCall( TimeSpan.FromSeconds( 2.0 ), new TimerCallback( Delete ) ); }
//			else { m_NextMorale = (DateTime.Now + TimeSpan.FromSeconds( 60 )); }
//		}

		[Constructable] 
		public HenchmanWizard( int myBody, int nMounted, double nSkills, int nStats ) : base( AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4 ) 
		{
			m_NextMorale = (DateTime.Now + TimeSpan.FromSeconds( 60 ));

			Name = "henchman";
			Body = myBody;
			RangeFight = 7;

			if ( Body == 401 ){ this.Female = true; }

			int nStr = (int)((nStats / 6) * 1);
			int nDex = (int)((nStats / 6) * 2);
			int nInt = (int)((nStats / 6) * 3);
			int nArmor = (int)(nStats / 7); if ( nArmor > 70 ){ nArmor = 70; }
			int nProtect = (int)(nStats / 4); if ( nProtect > 70 ){ nProtect = 70; }
			int nDamage = (int)(nStats / 7);

       	    SetStr( nStr );
            SetDex( nDex );
            SetInt( nInt );

            SetHits( nStr*2 );
            SetStam( nDex*2 );
            SetMana( nInt*2 );

			SetDamage( (int)(nDamage/2), nDamage );

			ControlSlots = 1;

			VirtualArmor = (int)(nStats / 5);

			SetDamageType( ResistanceType.Physical, 40 );
			SetDamageType( ResistanceType.Fire, 10 );
			SetDamageType( ResistanceType.Cold, 10 );
			SetDamageType( ResistanceType.Poison, 10 );
			SetDamageType( ResistanceType.Energy, 10 );

			SetResistance( ResistanceType.Physical, nProtect );
			SetResistance( ResistanceType.Fire, nArmor );
			SetResistance( ResistanceType.Cold, nArmor );
			SetResistance( ResistanceType.Poison, nArmor );
			SetResistance( ResistanceType.Energy, nArmor );

            SetSkill(SkillName.Magery, nSkills );
            SetSkill(SkillName.Psychology, nSkills );
            SetSkill(SkillName.Poisoning, nSkills );
            SetSkill(SkillName.Tactics, nSkills );
            SetSkill(SkillName.MagicResist, nSkills );
            SetSkill(SkillName.Focus, nSkills );
            SetSkill(SkillName.Meditation, nSkills );
            SetSkill(SkillName.Anatomy, nSkills );
            SetSkill(SkillName.Marksmanship, nSkills );
			SetSkill(SkillName.Healing, nSkills );

			if ( nMounted > 0 )
			{
				new HenchHorse().Rider = this;
				ActiveSpeed = 0.1;
				PassiveSpeed = 0.2;
			}
		}

		public DateTime m_NextResurrect;
		public static TimeSpan ResurrectDelay = TimeSpan.FromSeconds( 20.0 );

		public virtual void OfferResurrection( Mobile m )
		{
				Direction = GetDirectionTo( m );

				m.PlaySound( 0x214 );
				m.FixedEffect( 0x376A, 10, 16 );

				if ( m is PlayerMobile )
				{
					m.CloseGump( typeof( ResurrectCostGump ) );
					m.SendGump( new ResurrectCostGump( m, 1 ) );
				}
		}

		public virtual void OfferHeal( Mobile m )
		{
			Direction = GetDirectionTo( m );

				Say("Here's some help"); // You look like you need some healing my child.
				Say("In Vas Mani");

				m.PlaySound( 0x1F2 );
				m.FixedEffect( 0x376A, 9, 32 );

				m.Hits = (m.Hits + 60);
//				if( m.Poisoned )
//					new ArchCureSpell( this, null ).Cast();
				m_NextResurrect = DateTime.UtcNow + ResurrectDelay;
		}

		public override void OnMovement( Mobile m, Point3D oldLocation )
		{
			bool GoAway = HenchmanFunctions.OnMoving( m, oldLocation, this, m_NextMorale );
			if ( GoAway == true ){ Timer.DelayCall( TimeSpan.FromSeconds( 2.0 ), new TimerCallback( Delete ) ); }
			else { m_NextMorale = (DateTime.Now + TimeSpan.FromSeconds( 60 )); }
			if ( !m.Frozen && m is PlayerMobile && ControlMaster == m && DateTime.UtcNow >= m_NextResurrect && InRange( m, 6 ) && this.InLOS( m ) )
			{
				if ( !m.Alive )
				{
					m_NextResurrect = DateTime.UtcNow + ResurrectDelay;

					if ( m.Map == null || !m.Map.CanFit( m.Location, 16, false, false ) )
					{
						m.SendLocalizedMessage( 502391 ); // Thou can not be resurrected there!
					}
					else
					{
						OfferResurrection( m );
					}
				}
			}
		}
		
		public override void OnThink()
		{
			base.OnThink();
			Mobile m = this.ControlMaster;
			BaseCreature bc = null;
			foreach ( Mobile search in this.GetMobilesInRange( 16 ) )
			{
				if ( search is BaseCreature && search != this && this.CanSee( search ) )
				{
					bc = ((BaseCreature)search);
					break;
				}
			}
				if ( m.Alive && m.Hits < m.HitsMax - 30 && DateTime.UtcNow >= m_NextResurrect )
				{
					OfferHeal( (Mobile) m );
				}
				else if ( m.Hits >= m.HitsMax - 30 && DateTime.UtcNow >= m_NextResurrect )
				{
					foreach ( Mobile search in this.GetMobilesInRange( 6 ) ) 
						{
//							if ( search is BaseCreature && (search.Body == 0x191 || search.Body == 0x190 || search.Body == 0x25D || search.Body == 0x25E) && ((BaseCreature)search).ControlMaster == this.ControlMaster && ((BaseCreature)search).Controlled == true && search != this && this.CanSee( search ) && search.Hits < search.HitsMax - 10 )
//							{
//								OfferHeal( (Mobile) search );
//							}
							if ( search is BaseCreature && ((BaseCreature)search).ControlMaster == this.ControlMaster && ((BaseCreature)search).Controlled == true && search != this && this.CanSee( search ) && search.Alive && search.Hits < search.HitsMax - 10 )
							{
								OfferHeal( (Mobile) search );
							}
						}
				}
		}

        public override void OnSpeech( SpeechEventArgs e )
        {
            if (!e.Handled && Insensitive.Equals(e.Speech, "report"))
            {
                HenchmanFunctions.ReportStatus(this);
            }
            base.OnSpeech(e);
        }

		public override bool ClickTitle{ get{ return false; } }
		public override bool ShowFameTitle{ get{ return false; } }
		public override bool AlwaysAttackable{ get{ return true; } }
		public override bool ReacquireOnMovement{ get{ return true; } }
		public override bool InitialInnocent{ get{ return true; } }
		public override bool DeleteOnRelease{ get{ return true; } }
		public override bool DeleteCorpseOnDeath{ get{ return true; } }
		public override bool IsDispellable { get { return false; } }
		public override bool IsBondable{ get{ return false; } }
		public override bool CanBeRenamedBy( Mobile from ){ return false; }

		public override void OnGaveMeleeAttack( Mobile defender )
		{
			HenchmanFunctions.OnGaveAttack( this );
		}

        public override void OnDamagedBySpell(Mobile attacker)
        {
            base.OnDamagedBySpell(attacker);
			HenchmanFunctions.OnSpellAttack( this );
        }

		public override void OnGotMeleeAttack( Mobile defender )
		{
			HenchmanFunctions.OnGotAttack( this );
		}

		public override bool OnBeforeDeath()
		{
			HenchmanFunctions.OnDead( this );
			if ( !base.OnBeforeDeath() )
				return false;

			return true;
		}

		public override bool OnDragDrop( Mobile from, Item dropped )
		{
			HenchmanFunctions.OnGive( from, dropped, this );
			return base.OnDragDrop( from, dropped );
		}

		public HenchmanWizard( Serial serial ) : base( serial ) 
		{ 
		} 

		public override void Serialize( GenericWriter writer ) 
		{ 
			base.Serialize( writer ); 
			writer.Write( (int) 0 ); // version
			Loyalty = 100;
		} 

		public override void Deserialize( GenericReader reader ) 
		{ 
			base.Deserialize( reader ); 
			int version = reader.ReadInt();
			Timer.DelayCall( TimeSpan.FromSeconds( 5.0 ), new TimerCallback( Delete ) );
		} 
	} 
}   