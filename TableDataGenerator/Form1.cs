using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using MySql.Data.MySqlClient;
using System.Data.SqlClient;

namespace TableDataGenerator
{
    public partial class Form1 : Form
    {
        const UInt32 WARRIOR_CLASS = 1;
        const UInt32 PALADIN_CLASS = 2;
        const UInt32 MAGE_CLASS = 8;


        public Form1()
        {
            InitializeComponent();
        }
        

        private void btnConnect_Click(object sender, EventArgs e)
        {
            // connect to the database
            MySqlConnection conn;
            string myConnectionString;

            myConnectionString = "server=127.0.0.1;uid=root;" +
                "pwd=root;database=mangoszero;";

            try
            {
                conn = new MySql.Data.MySqlClient.MySqlConnection(myConnectionString);
                conn.Open();
                lblConnectionState.Text = "Connected";
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show(ex.Message);
                lblConnectionState.Text = "Failed to Connect";
            }


            // set text to show successfull connection

        }


        // ------====== Base Health ======------
        // The function works out the base health
        private int getBaseHealth(UInt32 iClass, int iCreatureLevel, string myConnectionString)
        {
            // Generate base health
            string sqlScript = "";

            // base health
            int iBaseHealth = 0;
            int iMinHealth = 0;
            int iMaxHealth = 0;
            double dAverageHealth = 0;
            double dHealthMultiplier = 0;

            sqlScript = " SELECT * FROM creature_template WHERE Rank = 0 AND UnitClass = " + iClass + " AND MinLevel = " + iCreatureLevel + " AND MinLevel = MaxLevel LIMIT 1 ";
            using (MySqlConnection connect = new MySqlConnection(myConnectionString))
            using (MySqlCommand cmd = new MySqlCommand(sqlScript, connect))
            {
                connect.Open();
                using (MySqlDataReader MySQLReader = cmd.ExecuteReader())
                {
                    while (MySQLReader.Read())
                    {
                        txtResults.Text += "" + iCreatureLevel + " " + MySQLReader.GetString("Entry") + " \r\n";

                        iMinHealth = MySQLReader.GetInt32("MinLevelHealth");
                        iMaxHealth = MySQLReader.GetInt32("MinLevelHealth");
                        dAverageHealth = (iMinHealth + iMaxHealth) / 2;
                        dHealthMultiplier = MySQLReader.GetDouble("HealthMultiplier");

                        iBaseHealth = Convert.ToInt32(iMinHealth / dHealthMultiplier);
                    }
                }
            } // Here the connection will be closed and disposed.  (and the command also)

            return iBaseHealth;
        }


        // ------====== Base Mana ======------
        // The function works out the base mana
        private int getBaseMana(UInt32 iClass, int iCreatureLevel, string myConnectionString)
        {
            // Generate base health
            string sqlScript = "";

            // base health
            int iBaseMana = 0;
            int iMinMana = 0;
            double dManaMultiplier = 0;

            sqlScript = " SELECT * FROM creature_template WHERE Rank = 0 AND UnitClass = " + iClass + " AND MinLevel = " + iCreatureLevel + " AND MinLevel = MaxLevel LIMIT 1 ";
            using (MySqlConnection connect = new MySqlConnection(myConnectionString))
            using (MySqlCommand cmd = new MySqlCommand(sqlScript, connect))
            {
                connect.Open();
                using (MySqlDataReader MySQLReader = cmd.ExecuteReader())
                {
                    while (MySQLReader.Read())
                    {
                        iMinMana = MySQLReader.GetInt32("MinLevelMana");
                        dManaMultiplier = MySQLReader.GetDouble("ManaMultiplier");

                        iBaseMana = Convert.ToInt32(iMinMana / dManaMultiplier);
                    }
                }
            } // Here the connection will be closed and disposed.  (and the command also)

            return iBaseMana;
        }


        // ------====== BASE DAMAGE ======------
        // The method works out the base damage
        private double getBaseDamage(UInt32 iClass, int iCreatureLevel, int iBaseMeleeAttackPower, string myConnectionString)
        {
            string sqlScript = "";

            double dBaseDamage = 0; // this is what we need to worl out
            int iBaseMeleeAttackTime = 0; // MeleeBaseAttackTime in creature_tempplate table
            int iMinMeleeDamage = 0;
            int iMaxMeleeDamage = 0;
            int iDamageMultiplier = 0;
            double dDamageVariance = 0;

            sqlScript = " SELECT * FROM creature_template WHERE Rank = 0 AND UnitClass = " + iClass + " AND MinLevel = " + iCreatureLevel + " AND MinLevel = MaxLevel LIMIT 1 ";
            using (MySqlConnection connect = new MySqlConnection(myConnectionString))
            using (MySqlCommand cmd = new MySqlCommand(sqlScript, connect))
            {
                connect.Open();
                using (MySqlDataReader MySQLReader = cmd.ExecuteReader())
                {
                    while (MySQLReader.Read())
                    {
                        iDamageMultiplier = MySQLReader.GetInt32("DamageMultiplier");

                        dDamageVariance = MySQLReader.GetDouble("DamageVariance");

                        iBaseMeleeAttackPower = MySQLReader.GetInt32("MeleeAttackPower");

                        iBaseMeleeAttackTime = MySQLReader.GetInt32("MeleeBaseAttackTime");

                        iMinMeleeDamage = MySQLReader.GetInt32("MinMeleeDmg");
                        iMaxMeleeDamage = MySQLReader.GetInt32("MaxMeleeDmg");


                        // REVERSE THE CALCULATION in order to acquire the base damage
                        // CalculatedMinMeleeDmg=ROUND(((BaseDamage * Damage Variance) + (Base Melee Attackpower / 14)) * (Base Attack Time/1000)) * Damage Multiplier


                        // (Base Damage * Damage Variance)
                        double dBaseDamage_x_DamageVariance = 0;
                        // (Base Attack Time/1000)
                        double dBaseAttackTime_DIV_1000 = 0;
                        // (Base Melee Attackpower / 14)
                        double iBaseMeleeAttackPower_DIV_14 = 0;
                        // ((BaseDamage * Damage Variance) + (Base Melee Attackpower / 14)) * (Base Attack Time/1000)) / (Base Attack Time/1000)
                        double dTotalOfBracketedCalculations_DIV_BaseAttackTimeDIV1000 = 0;
                        // (((BaseDamage * Damage Variance) + (Base Melee Attackpower / 14)) * (Base Attack Time/1000))
                        double dTotalOfBracketedCalculations = 0;

                        // MinMeleeDmg / DamageMultiplyer = OverallValue
                        dTotalOfBracketedCalculations = iMinMeleeDamage / iDamageMultiplier;

                        // BaseMeleeAtackTime / 1000 = RightMost
                        dBaseAttackTime_DIV_1000 = iBaseMeleeAttackTime / 1000;

                        // BaseMeleeAttackPower / 14 = Middle
                        iBaseMeleeAttackPower_DIV_14 = iBaseMeleeAttackPower / 14;

                        // OverallValue / Right = ((Middle + Left) = Average
                        dTotalOfBracketedCalculations_DIV_BaseAttackTimeDIV1000 = dTotalOfBracketedCalculations / dBaseAttackTime_DIV_1000;

                        dBaseDamage_x_DamageVariance = dTotalOfBracketedCalculations_DIV_BaseAttackTimeDIV1000 - iBaseMeleeAttackPower_DIV_14;

                        dBaseDamage = dBaseDamage_x_DamageVariance / dDamageVariance;
                        
                    }
                }
            } // Here the connection will be closed and disposed.  (and the command also)

            return dBaseDamage;
        }


        // ------====== Base Melee Attack Power ======------
        // The function works out the base melee attack power
        //
        // I'm not sure of the accuracy of this one, even though it is taken straight from the creature_tamplate table
        private int getBaseMeleeAttackPower(UInt32 iClass, int iCreatureLevel, string myConnectionString)
        {
            // Generate base  melee attack power
            string sqlScript = "";

            // base  melee attack power     
            int iBaseMeleeAttackPower = 0; // used for itself (BaseMeleeAttackPower) and for calculating BaseDamge

            // Generate base damage

            sqlScript = " SELECT * FROM creature_template WHERE Rank = 0 AND UnitClass = " + iClass + " AND MinLevel = " + iCreatureLevel + " AND MinLevel = MaxLevel LIMIT 1 ";
            
            using (MySqlConnection connect = new MySqlConnection(myConnectionString))
            using (MySqlCommand cmd = new MySqlCommand(sqlScript, connect))
            {
                connect.Open();
                using (MySqlDataReader MySQLReader = cmd.ExecuteReader())
                {
                    while (MySQLReader.Read())
                    {
                        iBaseMeleeAttackPower = MySQLReader.GetInt32("MeleeAttackPower");
                    }
                }
            } // Here the connection will be closed and disposed.  (and the command also)
            
            return iBaseMeleeAttackPower;
        }


        // ------====== BASE RANGED ATTACK POWER ======------
        // The method works out the base ranged attack power
        private int getBaseRangedAttackPower(UInt32 iClass, int iCreatureLevel, string myConnectionString)
        {
            string sqlScript = "";

            int iBaseRangedAttackPower = 0; // this is what we need to worl out
            int iBaseRangedAttackTime = 0; // MeleeBaseAttackTime in creature_tempplate table
            int iMinRangedDamage = 0;
            int iMaxRangedDamage = 0;
            double dAverageDamage = 0;
            int iDamageMultiplier = 0;
            double dDamageVariance = 0;

            double dBaseDamage = 0;


            sqlScript = " SELECT * FROM creature_template WHERE Rank = 0 AND UnitClass = " + iClass + " AND MinLevel = " + iCreatureLevel + " AND MinLevel = MaxLevel LIMIT 1 ";
            using (MySqlConnection connect = new MySqlConnection(myConnectionString))
            using (MySqlCommand cmd = new MySqlCommand(sqlScript, connect))
            {
                connect.Open();
                using (MySqlDataReader MySQLReader = cmd.ExecuteReader())
                {
                    while (MySQLReader.Read())
                    {
                        iDamageMultiplier = MySQLReader.GetInt32("DamageMultiplier");

                        dDamageVariance = MySQLReader.GetDouble("DamageVariance");

                        //       iBaseRangedAttackPower = reader.GetInt32("RangedAttackPower");   // this cannot be what we want!  only values of 0 and 100

                        iBaseRangedAttackTime = MySQLReader.GetInt32("RangedBaseAttackTime");
                        if (iBaseRangedAttackTime == 0)
                            iBaseRangedAttackTime = 1000;   // should we really have to do this ????
                        // Vanilla has all BaseRangedAttackTime > 0, but
                        // TBC has many set to 0

                        iMinRangedDamage = MySQLReader.GetInt32("MinRangedDmg");
                        iMaxRangedDamage = MySQLReader.GetInt32("MaxRangedDmg");
                        dAverageDamage = (iMinRangedDamage + iMaxRangedDamage) / 2;


                        if (iDamageMultiplier == 0 || dDamageVariance == 0 || dAverageDamage == 0)
                        {
                            txtResults.Text += "\r\n A REQUIRED VALUE WAS 0!!! \r\n";
                            return 0; // calculation cannot be performed
                        }


                        // REVERSE THE CALCULATION in order to acquire the ranged attack power

                        // EQUATION
                        // CalculatedMinRangedDmg=ROUND(((BaseDamage * Damage Variance)+(BaseRangedAttackPower/14))*(Base Ranged Attack Time/1000)) * Damage Multiplier

                        // Left hand side expression (of the equation)
                        // (((BaseDamage * Damage Variance)+(BaseRangedAttackPower/14))*(Base Ranged Attack Time/1000)) * Damage Multiplier

                        // (((BaseDamage * Damage Variance)+(BaseRangedAttackPower/14))*(Base Ranged Attack Time/1000))
                        double dTotal = 0;

                        // Expression 1
                        // (Base Ranged Attack Time/1000)
                        double dExpression1 = 0;

                        // Expression 2
                        // (BaseDamage * Damage Variance)
                        double dExpression2 = 0;

                        // Expression 3
                        // (BaseRangedAttackPower/14)
                        double dExpression3 = 0;   // uncomment this if using a ratio other than 50:50

                        // Expression 4
                        // ((BaseDamage * Damage Variance)+(BaseRangedAttackPower/14))
                        double dExpression4 = 0;


                        // (((BaseDamage * Damage Variance)+(BaseRangedAttackPower/14))*(Base Ranged Attack Time/1000))
                        dTotal = dAverageDamage / iDamageMultiplier;

                        // (Base Ranged Attack Time/1000)
                        dExpression1 = iBaseRangedAttackTime / 1000;

                        // ((BaseDamage * Damage Variance)+(BaseRangedAttackPower/14))
                        dExpression4 = dTotal / dExpression1;

                        // Next we split the result held in dExpression4 in two ( div by 2), that we we can acquire the unknown values
                        // It may be that another ratio be used instead of 50:50, such as 70:30 or 20:80. 
                        dExpression2 = dExpression4 / 2;
                        // with a ratio of 50:50 used, we only need to use dExpression1, but
                        // with any other ratio we would then also use dExpression2 e.g.
                        // dExpression2 = dExpression4 / 4;        // 25% of total value
                        // dExpression3 = dExpression4 / 4 * 3;    // 75% of total value

                        dBaseDamage = dExpression2 / 0.34;

                        iBaseRangedAttackPower = (int)(dExpression2 * 14);

                    }
                }
            } // Here the connection will be closed and disposed.  (and the command also)
            
            return iBaseRangedAttackPower;
        }



        private void btnGenerateScript_Click(object sender, EventArgs e)
        {
            // connect to the database
            string myConnectionString;

            myConnectionString = "server=127.0.0.1;uid=root;" +
                "pwd=root;database=mangoszero;";

            // base health
            int iBaseHealth = 0;

            // base mana
            int iBaseMana = 0;

            double dBaseDamage = 0;

            int iBaseMeleeAttackPower = 0; // used for itself (BaseMeleeAttackPower) and for calculating BaseDamge

            int iBaseRangedAttackPower = 0;

            int iBaseArmour = 0; // ?????????????????? no info on how to obtain this!

            // SET THIS TO 0 for all records
                
            // Class: Warrior (1)
            // ------------------
            for (int iCreatureLevel = 1; iCreatureLevel <= 63; iCreatureLevel++)
            {                
                // Generate base health
                iBaseHealth = getBaseHealth(MAGE_CLASS, iCreatureLevel, myConnectionString);
                txtResults.Text += "\r\n BASE HEALTH: " + iBaseHealth + "\r\n";

                // Generate base mana - will be 0 for all records
                iBaseMana = 0;

                // Generate base damage
                dBaseDamage = getBaseDamage(MAGE_CLASS, iCreatureLevel, iBaseMeleeAttackPower, myConnectionString);
                txtResults.Text += " BASE DMG: " + dBaseDamage + " --- \r\n";

                // Generate base melee attack power
                iBaseMeleeAttackPower = getBaseMeleeAttackPower(MAGE_CLASS, iCreatureLevel, myConnectionString);
                txtResults.Text += " BASE MELEE ATTACK POWER: " + iBaseMeleeAttackPower + " --- \r\n";

                // Generate base ranged attack power
                iBaseRangedAttackPower = getBaseRangedAttackPower(MAGE_CLASS, iCreatureLevel, myConnectionString);
                txtResults.Text += "\r\n BASE RANGED ATTACK POWER: " + iBaseRangedAttackPower + "\r\n";

                // Generate base armour 
                // ?????? no info on how to do this!!!

            }



            // Class: Paladin (2)
            
            // Class: Rogue (4)

            // Class: Mage (8)


            

        }
    }
}
