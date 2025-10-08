using System; 
using System.Collections; 
using Server;
using Server.Misc; 
using Server.Items; 
using Server.Mobiles;
using Server.Gumps;
using Server.Targeting;

namespace Server.Mobiles 
{ 
	public class BaseRed : BaseCreature 
	{ 
		public BaseRed(AIType ai, FightMode fm, int PR, int FR, double AS, double PS) : base( ai, fm, PR, FR, AS, PS )
		{
			SpeechHue = Utility.RandomDyedHue(); 
			Hue = Utility.RandomSkinHue();
			RangePerception = BaseCreature.DefaultRangePerception;
			Criminal = true;
		}

		public override bool IsEnemy( Mobile m )
		{
            if ( m is BaseRed || m is Citizens || m is PlayerVendor || m is TownHerald )
				return false;

			if ( m is PlayerMobile && m.Criminal || m.Kills >= 5 )
				return false;

			//if ( m is BaseCreature )
			//{
			//	BaseCreature c = (BaseCreature)m;
			//}	
			return true;
		}

		public virtual bool HealsYoungPlayers{ get{ return false; } }

		public virtual bool CheckResurrect( Mobile m )
		{
			return false;
		}



		public virtual void OfferResurrection( Mobile m )
		{

		}

		public virtual void OfferHeal( PlayerMobile m )
		{

		}

		public override void OnMovement( Mobile m, Point3D oldLocation )
		{

		}

		public override void OnThink()
		{
			base.OnThink();
			
			// Chug pots
			if ( this.Poisoned )
			{
				GreaterCurePotion m_CPot = (GreaterCurePotion)this.Backpack.FindItemByType( typeof ( GreaterCurePotion ) );
				if ( m_CPot != null )
					m_CPot.Drink( this );
			}

			if ( this.Hits <= (this.HitsMax * .7) ) // Will try to use heal pots if he's at or below 70% health
			{
				GreaterHealPotion m_HPot = (GreaterHealPotion)this.Backpack.FindItemByType( typeof ( GreaterHealPotion ) );
				if ( m_HPot != null )
					m_HPot.Drink( this );
			}
			
			if ( this.Stam <= (this.StamMax * .25) ) // Will use a refresh pot if he's at or below 25% stam
			{
				TotalRefreshPotion m_RPot = (TotalRefreshPotion)this.Backpack.FindItemByType( typeof ( TotalRefreshPotion) );
				if ( m_RPot != null )
					m_RPot.Drink( this );
			}
		}

		public BaseRed( Serial serial ) : base( serial ) 
		{ 
		} 

		public override void Serialize( GenericWriter writer ) 
		{ 
			base.Serialize( writer ); 

			writer.Write( (int) 0 ); // version 
		} 

		public override void Deserialize( GenericReader reader ) 
		{ 
			base.Deserialize( reader ); 

			int version = reader.ReadInt(); 
		} 
	} 
}   