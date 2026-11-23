# 5.1.52426.0 布局表偏移溯源审计

由 `CoH3/dev/SkyEye/scripts/audit_role_provenance.py` 自动生成（只读）。

- 表内角色总数：37
- 静态清单直接支撑：16
- 既有重定位报告支撑：21
- 无来源条目：0 []

| 旧偏移 | 新 RVA | 来源 | 说明 |
| --- | --- | --- | --- |
| 0x6BB0020 | `0x6bb1ed0` | 静态清单：feature:resource_root_getter->rip_relative | resource singleton / getter and live name/index table |
| 0x5E0BECC | `0x5e0decc` | 静态清单：feature:world_offset->rip_relative | world offset / RIP read and live world |
| 0x6185EB0 | `0x6187e40` | 静态清单：feature:role_0xcd0f1d->rip_relative；feature:role_0xcd0f60->rip_relative；feature:component_descriptor_resolver->rip_relative；feature:arena_helper_anchor->rip_relative | arena table / static resolver and live arena |
| 0x6185EE0 | `0x6187e70` | 静态清单：feature:tls_component_pool->image_indexed_displacement | TLS-indexed component pool array (index 0 snapshot) |
| 0x6185FC8 | `0x6187f58` | 静态清单：feature:camera_registry->rip_relative | GameUICore list owner |
| 0x6186220 | `0x61881b0` | 静态清单：feature:colour_property_registry->rip_relative | config registry / key 0x320000 |
| 0x6B9C1C8 | `0x6b9e078` | 静态清单：feature:ui_singleton_read->rip_relative；feature:ui_singleton_store->rip_relative | UI singleton / GameUIAppWindow |
| 0x3DB01F8 | `0x3db1068` | 报告：static/REPORT.md | GameUICore, not CameraEntity |
| 0x3DAFEF8 | `0x3db0d68` | 报告：static/REPORT.md | TeamColourManager |
| 0x3DD1FA0 | `0x3dd4bf0` | 报告：static/REPORT.md | GameUIAppWindow |
| 0x3E7A150 | `0x3e7b160` | 报告：static/REPORT.md | MapControl |
| 0x3E7A800 | `0x3e7b810` | 报告：static/REPORT.md | TacticalMapControl |
| 0x3EAB6F0 | `0x3eac6b0` | 报告：static/REPORT.md | AnvilRTSHUDPage |
| 0x3D72280 | `0x3d73280` | 报告：static/REPORT.md | TranslateTransform |
| 0x3D723A0 | `0x3d733a0` | 报告：static/REPORT.md | MatrixTransform |
| 0x3D72AE0 | `0x3d73ae0` | 报告：static/REPORT.md | TransformGroup |
| 0x3DD00B0 | `0x3dd2d00` | 报告：static/REPORT.md | RotateTransform |
| 0x3DD0228 | `0x3dd2e78` | 报告：static/REPORT.md | ScaleTransform |
| 0x6AB2078 | `0x6ab4018` | 报告：static/REPORT.md | Matrix property pointer, wrapper +32; ID 69 |
| 0x6ACD388 | `0x6acf328` | 报告：static/REPORT.md | PlayableWidth pointer; ID 874 |
| 0x6ACD3B0 | `0x6acf350` | 报告：static/REPORT.md | PlayableHeight pointer; ID 875 |
| 0x3D97CB0 | `0x3d98bf0` | 静态清单：role_candidates:SpawnProductionItem[static_identity_plus_agent1_runtime_role]；feature:role_0x7e1e97->rip_relative | SpawnProductionItem / RTTI and live production chain |
| 0x3D93B00 | `0x3d94a40` | 静态清单：role_candidates:TimedAbility[static_identity_plus_agent1_runtime_role]；feature:role_0x76568f->rip_relative | TimedAbility / RTTI, constructor, live mortar barrage |
| 0x3E49968 | `0x3e4aa38` | 静态清单：role_candidates:ProjectileDynamics[static_role_candidate]；feature:projectile_dynamics_install->rip_relative | ProjectileDynamics / static RTTI candidate; event fields pending |
| 0x3E48548 | `0x3e49618` | 静态清单：role_candidates:ProjectileExt[static_identity_plus_agent1_runtime_role]；feature:projectile_vtable_install->rip_relative | ProjectileExt / RTTI and live mortar projectile chain |
| 0x3D917A8 | `0x3d926e0` | 静态清单：role_candidates:MoverDynamics[static_role_candidate_not_observed]；feature:mover_vtable_install->rip_relative | MoverDynamics / RTTI and live loiter instance |
| 0xCD09F0 | `0xcd0f10` | 静态清单：role_candidates:MoverDynamics virtual +344[static_callable_candidate] | MoverDynamics vtable slot +344 / mode resolver |
| 0x3D90FF0 | `0x3d92058` | 静态清单：role_candidates:SquadUpgradeProductionItem[static_role_candidate_not_observed]；feature:role_0x54c783->rip_relative | SquadUpgradeProductionItem |
| 0x3D910F8 | `0x3d92160` | 静态清单：role_candidates:EntityUpgradeProductionItem[static_role_candidate_not_observed]；feature:role_0x54c7c4->rip_relative | EntityUpgradeProductionItem |
| 0x6288842 | `0x628a7e2` | 报告：components/component-report.md、production/kwk38-sequence-report.md | SimInventoryItemExt |
| 0x628884A | `0x628a7ea` | 静态清单：feature:health_lookup->image_indexed_displacement | HealthExt |
| 0x628884B | `0x628a7eb` | 报告：components/component-report.md | ConstructionExt |
| 0x6288850 | `0x628a7f0` | 报告：components/component-report.md | CombatExt |
| 0x6288851 | `0x628a7f1` | 报告：components/component-report.md、production/hq-upgrade-report-01.md | ProductionQueueExt |
| 0x6288852 | `0x628a7f2` | 报告：components/component-report.md、components/role-report.md | no active sample yet; lookup ID only |
| 0x6288855 | `0x628a7f5` | 报告：components/component-report.md | AbilityExt |
| 0x6288891 | `0x628a831` | 报告：components/component-report.md | WeaponExt |
