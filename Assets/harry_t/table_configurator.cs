#define HT8B_DRAW_REGIONS

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[ExecuteInEditMode]
public class table_configurator : MonoBehaviour
{

[HideInInspector]
public ht8b_collision_data_t cdata;

float k_MINOR_REGION_CONST;
float r_k_CUSHION_RADIUS;

const float k_BALL_RADIUS     = 0.03f;

Vector3 k_vA = new Vector3();
Vector3 k_vB = new Vector3();
Vector3 k_vC = new Vector3();
Vector3 k_vD = new Vector3();

Vector3 k_vX = new Vector3();
Vector3 k_vY = new Vector3();
Vector3 k_vZ = new Vector3();
Vector3 k_vW = new Vector3();

Vector3 k_pK = new Vector3();
Vector3 k_pL = new Vector3();
Vector3 k_pM = new Vector3();
Vector3 k_pN = new Vector3();
Vector3 k_pO = new Vector3();
Vector3 k_pP = new Vector3();
Vector3 k_pQ = new Vector3();
Vector3 k_pR = new Vector3();
Vector3 k_pT = new Vector3();
Vector3 k_pS = new Vector3();
Vector3 k_pU = new Vector3();
Vector3 k_pV = new Vector3();

Vector3 k_vA_vD = new Vector3();
Vector3 k_vA_vD_normal = new Vector3();

Vector3 k_vB_vY = new Vector3();
Vector3 k_vB_vY_normal = new Vector3();

Vector3 k_vC_vZ_normal = new Vector3();

Vector3 k_vA_vB_normal = new Vector3( 0.0f, 0.0f, -1.0f );
Vector3 k_vC_vW_normal = new Vector3( -1.0f, 0.0f, 0.0f );

//public Vector3 k_vE = new Vector3(1.087f,0.0f,0.627f);
//public Vector3 k_vF = new Vector3(0.0f,0.0f,0.665f);

// Stub
void _phy_bounce_cushion( int id, Vector3 N ) {}

string _encode_table_collider()
{
   return "";
}

void _phy_table_init()
{
   // Handy values
   k_MINOR_REGION_CONST = cdata.k_TABLE_WIDTH - cdata.k_TABLE_HEIGHT;

   // Major source vertices
   k_vA.x = cdata.k_POCKET_RADIUS * 0.92f;
   k_vA.z = cdata.k_TABLE_HEIGHT;

   k_vB.x = cdata.k_TABLE_WIDTH-cdata.k_POCKET_RADIUS;
   k_vB.z = cdata.k_TABLE_HEIGHT;

   k_vC.x = cdata.k_TABLE_WIDTH;
   k_vC.z = cdata.k_TABLE_HEIGHT-cdata.k_POCKET_RADIUS;

   k_vD.x = k_vA.x - 0.016f;
   k_vD.z = k_vA.z + 0.06f;

   // Aux points
   k_vX = k_vD + Vector3.forward;
   k_vW = k_vC;
   k_vW.z = 0.0f;

   k_vY = k_vB;
   k_vY.x += 1.0f;
   k_vY.z += 1.0f;

   k_vZ = k_vC;
   k_vZ.x += 1.0f;
   k_vZ.z += 1.0f;

   // Normals
   k_vA_vD = k_vD - k_vA;
   k_vA_vD = k_vA_vD.normalized;
   k_vA_vD_normal.x = -k_vA_vD.z;
   k_vA_vD_normal.z =  k_vA_vD.x;

   k_vB_vY = k_vB - k_vY;
   k_vB_vY = k_vB_vY.normalized;
   k_vB_vY_normal.x = -k_vB_vY.z;
   k_vB_vY_normal.z =  k_vB_vY.x;

   k_vC_vZ_normal = -k_vB_vY_normal;

   // Minkowski difference
   k_pN = k_vA;
   k_pN.z -= r_k_CUSHION_RADIUS;

   k_pM = k_vA + k_vA_vD_normal * r_k_CUSHION_RADIUS;
   k_pL = k_vD + k_vA_vD_normal * r_k_CUSHION_RADIUS;

   k_pK = k_vD;
   k_pK.x -= r_k_CUSHION_RADIUS;

   k_pO = k_vB;
   k_pO.z -= r_k_CUSHION_RADIUS;
   k_pP = k_vB + k_vB_vY_normal * r_k_CUSHION_RADIUS;
   k_pQ = k_vC + k_vC_vZ_normal * r_k_CUSHION_RADIUS;
   
   k_pR = k_vC;
   k_pR.x -= r_k_CUSHION_RADIUS;

   k_pT = k_vX;
   k_pT.x -= r_k_CUSHION_RADIUS;

   k_pS = k_vW;
   k_pS.x -= r_k_CUSHION_RADIUS;

   k_pU = k_vY + k_vB_vY_normal * r_k_CUSHION_RADIUS;
   k_pV = k_vZ + k_vC_vZ_normal * r_k_CUSHION_RADIUS;
  
   k_pS = k_vW;
   k_pS.x -= r_k_CUSHION_RADIUS;
}

string _obj_vec_str( Vector3 v )
{
   return $"v {v.x} {v.y} {v.z}\n";
}

void _phy_draw_circle( Vector3 at, float r, Color colour )
{
   Vector3 last = at + Vector3.forward * r;
   Vector3 cur = Vector3.zero;

   for( int i = 1; i < 32; i ++ )
   {
      float angle = ((float)i/31.0f)*6.283185307179586f;
      cur.x = at.x + Mathf.Sin( angle ) * r;
      cur.z = at.z + Mathf.Cos( angle ) * r;

      _drawline_multi( last, cur, colour );
      last = cur;
   }
}

void _drawline_applyparent( Vector3 from, Vector3 to, Color colour )
{
    Debug.DrawLine( this.transform.parent.TransformPoint( from ), this.transform.parent.TransformPoint( to ), colour );
}

// Reflective, stacked by n
void _drawline_multi( Vector3 from, Vector3 to, Color colour )
{
   Vector3 reflect_x = new Vector3( -1.0f, 1.0f, 1.0f );
   Vector3 reflect_z = new Vector3( 1.0f, 1.0f, -1.0f );
   Vector3 reflect_xz = Vector3.Scale( reflect_x, reflect_z );

   for( int n = -4; n <= 4; n ++ )
   {
      Vector3 height = Vector3.up * (float)n * 0.006f;

      _drawline_applyparent( from + height, to + height, colour );
      _drawline_applyparent( Vector3.Scale( from, reflect_x ) + height,  Vector3.Scale( to, reflect_x ) + height, colour );
      _drawline_applyparent( Vector3.Scale( from, reflect_z ) + height,  Vector3.Scale( to, reflect_z ) + height, colour );
      _drawline_applyparent( Vector3.Scale( from, reflect_xz ) + height,  Vector3.Scale( to, reflect_xz ) + height, colour );
   }
}

Vector3 _sign_pos = new Vector3(0.0f,1.0f,0.0f);
void _phy_ball_table_new()
{
   Vector3 A, N, a_to_v;
   float dot;

   A = this.transform.localPosition;

   int id = 0;
   
   _sign_pos.x = Mathf.Sign( A.x );
   _sign_pos.z = Mathf.Sign( A.z );

   A = Vector3.Scale( A, _sign_pos );

#if HT8B_DRAW_REGIONS

   r_k_CUSHION_RADIUS = cdata.k_CUSHION_RADIUS-k_BALL_RADIUS;

   _phy_table_init();

   _drawline_multi( k_pT, k_pK, Color.yellow );
   _drawline_multi( k_pK, k_pL, Color.yellow );
   _drawline_multi( k_pL, k_pM, Color.yellow );
   _drawline_multi( k_pM, k_pN, Color.yellow );
   _drawline_multi( k_pN, k_pO, Color.yellow );
   _drawline_multi( k_pO, k_pP, Color.yellow );
   _drawline_multi( k_pP, k_pU, Color.yellow );

   _drawline_multi( k_pV, k_pQ, Color.yellow );
   _drawline_multi( k_pQ, k_pR, Color.yellow );
   _drawline_multi( k_pR, k_pS, Color.yellow );

   if( _phy_ball_pockets() )
   {
      _phy_draw_circle( cdata._cornerpocket, cdata.k_INNER_RADIUS, Color.green );
      _phy_draw_circle( cdata._sidepocket, cdata.k_INNER_RADIUS, Color.green );
   }
   else
   {
      _phy_draw_circle( cdata._cornerpocket, cdata.k_INNER_RADIUS, Color.red );
      _phy_draw_circle( cdata._sidepocket, cdata.k_INNER_RADIUS, Color.red );
   }

   r_k_CUSHION_RADIUS = cdata.k_CUSHION_RADIUS;
   _phy_table_init();
#endif

   if( A.x > k_vA.x ) // Major Regions
   {
      if( A.x > A.z + k_MINOR_REGION_CONST ) // Minor B
      {
         if( A.z < cdata.k_TABLE_HEIGHT-cdata.k_POCKET_RADIUS )
         {
            // Region H
#if HT8B_DRAW_REGIONS
            _drawline_applyparent( new Vector3( 0.0f, 0.0f, 0.0f ), new Vector3( cdata.k_TABLE_WIDTH, 0.0f, 0.0f ), Color.red );
            _drawline_applyparent( k_vC, k_vC + k_vC_vW_normal, Color.red );
#endif
            if( A.x > cdata.k_TABLE_WIDTH - cdata.k_CUSHION_RADIUS )
            {
               // Static resolution
               A.x = cdata.k_TABLE_WIDTH - cdata.k_CUSHION_RADIUS;

               // Dynamic
               _phy_bounce_cushion( id, Vector3.Scale( k_vC_vW_normal, _sign_pos ) );
            }
         }
         else
         {
            a_to_v = A - k_vC;

            if( Vector3.Dot( a_to_v, k_vB_vY ) > 0.0f )
            {
               // Region I ( VORONI )
#if HT8B_DRAW_REGIONS
               _drawline_applyparent( k_vC, k_pR, Color.green );
               _drawline_applyparent( k_vC, k_pQ, Color.green );
#endif
               if( a_to_v.magnitude < cdata.k_CUSHION_RADIUS )
               {
                  // Static resolution
                  N = a_to_v.normalized;
                  A = k_vC + N * cdata.k_CUSHION_RADIUS;

                  // Dynamic
                  _phy_bounce_cushion( id, Vector3.Scale( N, _sign_pos ) );
               }
            }
            else
            {
               // Region J
#if HT8B_DRAW_REGIONS
               _drawline_applyparent( k_vC, k_vB, Color.red );
               _drawline_applyparent( k_pQ, k_pV, Color.blue );
#endif
               a_to_v = A - k_pQ;

               if( Vector3.Dot( k_vC_vZ_normal, a_to_v ) < 0.0f )
               {
                  // Static resolution
                  dot = Vector3.Dot( a_to_v, k_vB_vY );
                  A = k_pQ + dot * k_vB_vY;

                  // Dynamic
                  _phy_bounce_cushion( id, Vector3.Scale( k_vC_vZ_normal, _sign_pos ) );
               }
            }
         }
      }
      else // Minor A
      {
         if( A.x < k_vB.x )
         {
            // Region A
#if HT8B_DRAW_REGIONS
            _drawline_applyparent( k_vA, k_vA + k_vA_vB_normal, Color.red );
            _drawline_applyparent( k_vB, k_vB + k_vA_vB_normal, Color.red );
#endif
            if( A.z > k_pN.z )
            { 
               // Static resolution
               A.z = k_pN.z;

               // Dynamic
               _phy_bounce_cushion( id, Vector3.Scale( k_vA_vB_normal, _sign_pos ) );
            }
         }
         else
         {
            a_to_v = A - k_vB;

            if( Vector3.Dot( a_to_v, k_vB_vY ) > 0.0f )
            {
               // Region F ( VERONI )
#if HT8B_DRAW_REGIONS
               _drawline_applyparent( k_vB, k_pO, Color.green );
               _drawline_applyparent( k_vB, k_pP, Color.green );
#endif
               if( a_to_v.magnitude < cdata.k_CUSHION_RADIUS )
               {
                  // Static resolution
                  N = a_to_v.normalized;
                  A = k_vB + N * cdata.k_CUSHION_RADIUS;

                  // Dynamic
                  _phy_bounce_cushion( id, Vector3.Scale( N, _sign_pos ) );
               }
            }
            else
            {
               // Region G
#if HT8B_DRAW_REGIONS
               _drawline_applyparent( k_vB, k_vC, Color.red );
               _drawline_applyparent( k_pP, k_pU, Color.blue );
#endif
               a_to_v = A - k_pP;

               if( Vector3.Dot( k_vB_vY_normal, a_to_v ) < 0.0f )
               {
                  // Static resolution
                  dot = Vector3.Dot( a_to_v, k_vB_vY );
                  A = k_pP + dot * k_vB_vY;

                  // Dynamic
                  _phy_bounce_cushion( id, Vector3.Scale( k_vB_vY_normal, _sign_pos ) );
               }
            }
         }
      }
   }
   else
   {
      a_to_v = A - k_vA;

      if( Vector3.Dot( a_to_v, k_vA_vD ) > 0.0f )
      {
         a_to_v = A - k_vD;

         if( Vector3.Dot( a_to_v, k_vA_vD ) > 0.0f )
         {
            if( A.z > k_pK.z )
            {
               // Region E
#if HT8B_DRAW_REGIONS
               _drawline_applyparent( k_vD, k_vD + k_vC_vW_normal, Color.red );
#endif
               if( A.x > k_pK.x )
               {
                  // Static resolution
                  A.x = k_pK.x;

                  // Dynamic
                  _phy_bounce_cushion( id, Vector3.Scale( k_vC_vW_normal, _sign_pos ) );
               }
            }
            else
            {
               // Region D ( VORONI )
#if HT8B_DRAW_REGIONS
               _drawline_applyparent( k_vD, k_vD + k_vC_vW_normal, Color.green );
               _drawline_applyparent( k_vD, k_vD + k_vA_vD_normal, Color.green );
#endif
               if( a_to_v.magnitude < cdata.k_CUSHION_RADIUS )
               {
                  // Static resolution
                  N = a_to_v.normalized;
                  A = k_vD + N * cdata.k_CUSHION_RADIUS;

                  // Dynamic
                  _phy_bounce_cushion( id, Vector3.Scale( N, _sign_pos ) );
               }
            }
         }
         else
         {
            // Region C
#if HT8B_DRAW_REGIONS
            _drawline_applyparent( k_vA, k_vA + k_vA_vD_normal, Color.red );
            _drawline_applyparent( k_vD, k_vD + k_vA_vD_normal, Color.red );
            _drawline_applyparent( k_pL, k_pM, Color.blue );
#endif
            a_to_v = A - k_pL;

            if( Vector3.Dot( k_vA_vD_normal, a_to_v ) < 0.0f )
            {
               // Static resolution
               dot = Vector3.Dot( a_to_v, k_vA_vD );
               A = k_pL + dot * k_vA_vD;

               // Dynamic
               _phy_bounce_cushion( id, Vector3.Scale( k_vA_vD_normal, _sign_pos ) );
            }
         }
      }
      else
      {
         // Region B ( VORONI )
#if HT8B_DRAW_REGIONS
         _drawline_applyparent( k_vA, k_vA + k_vA_vB_normal, Color.green );
         _drawline_applyparent( k_vA, k_vA + k_vA_vD_normal, Color.green );
#endif
         if( a_to_v.magnitude < cdata.k_CUSHION_RADIUS )
         {
            // Static resolution
            N = a_to_v.normalized;
            A = k_vA + N * cdata.k_CUSHION_RADIUS;

            // Dynamic
            _phy_bounce_cushion( id, Vector3.Scale( N, _sign_pos ) );
         }
      }
   }

   A = Vector3.Scale( A, _sign_pos );

   this.transform.localPosition = A;
}

bool _phy_ball_pockets()
{
   Vector3 A;

   A = this.transform.localPosition;
   
   _sign_pos.x = Mathf.Sign( A.x );
   _sign_pos.z = Mathf.Sign( A.z );

   A = Vector3.Scale( A, _sign_pos );

   if( Vector3.Distance( A, cdata._cornerpocket ) < cdata.k_INNER_RADIUS )
   {
      return true;
   }

   if( Vector3.Distance( A, cdata._sidepocket ) < cdata.k_INNER_RADIUS )
   {
      return true;
   }

   if( A.z > cdata._sidepocket.z )
   {
      return true;
   }

   if( A.z > -A.x + cdata._cornerpocket.x + cdata._cornerpocket.z )
   {
      return true;
   }

   return false;
}

// Update is called once per frame
void Update()
{
	_phy_ball_table_new();
}

void Start()
{
}
}
