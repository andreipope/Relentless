# import xmlrunner
# from appium import webdriver

# from altunityrunner import AltrunUnityDriver, NotFoundException
# from .pages.area_bar_popup_page import Area_Bar_Popup_Page
# from .pages.base import CZBTests
# from .pages.forgot_password_page import Forgot_Password_Page
# from .pages.login_popup_page import Login_Popup_Page
# from .pages.main_menu_page import Main_Menu_Page
# from .pages.register_popup_page import Regitration_Popup_Page
# from .pages.succes_forgot_page import Succes_Forgot_Page
# from .pages.wait_page import Wait_Page
# from .pages.side_menu_popup_page import Side_Menu_Popup_Page
# from .pages.forgot_password_page import Forgot_Password_Page
# from .pages.horde_editing_page import Horde_Editing_Page
# from .pages.overlord_selection_page import Overlord_Selection_Page
# from .pages.overlord_ability_popup_page import Overlord_Ability_Popup_Page
# from .pages.deck_rename_tab_page import Deck_Rename_Tab_Page
# from .pages.battle_mode_selection_page import Battle_Mode_Selection_Page
# from .pages.deck_selection_page import Deck_Selection_Page
# from .pages.tutorial_popup_page import Tutorial_Popup_Page
# from .pages.open_packs_page import Open_Packs_Page
# from .pages.battle_mode_selection_pop_up_page import Battle_Mode_Selection_Popup_Page
# from .pages.settings_page import Settings_Page
# from .pages.credits_page import Credits_Page
# from .pages.my_cards_page import My_Cards_Page
# from .pages.shop_page import Shop_Page

    
# class NavigationalModel(CZBTests):
#     def setUpModel(self):
#         self.page=super(NavigationalModel,self)
#         self.setUp()
#     ##Vertice
#     def tutorial_popup_page (self): 
#         self.page=Tutorial_Popup_Page(self.altdriver)
#     def side_menu_page (self):
#         self.page=Side_Menu_Popup_Page(self.altdriver)
#     def main_menu_page (self): 
#         self.page=Main_Menu_Page(self.altdriver)

#     def area_bar_page (self): 
#         self.page=Area_Bar_Popup_Page(self.altdriver)

#     def my_shop_menu_page (self): 
#         self.page=Shop_Page(self.altdriver,self.driver)
#     def my_decks_page (self): 
#         self.page=Deck_Selection_Page(self.altdriver)
#     def my_packs_page (self): 
#         self.page=Open_Packs_Page(self.altdriver,self.driver)
#     def my_cards_page (self): 
#         self.page=My_Cards_Page(self.altdriver,self.driver)
#     ##Edge
#     def press_no_button (self): 
#         self.page.press_No_button()

#     def select_area_bar (self): 
#         self.page=Area_Bar_Popup_Page(self.altdriver)
#     def select_side_menu (self): 
#         #no action needed
#         pass
#     def press_my_decks_button (self): 
#         self.page.press_my_decks_button()
#     def press_main_menu_page (self): 
#         self.page.press_battle_button()
#     def press_my_cards_button (self): 
#         self.page.press_my_cards_button()
#     def press_my_shop_button (self): 
#         self.page.press_shop_button()
#     def press_open_packs_button (self): 
#         self.page.press_open_packs_button()
    
#     def return_from_login_model (self): 
#         pass
# class LoginModel(CZBTests):
#     def setUpModel(self):
#         self.page=super(LoginModel,self)
#         self.setUp()

#     ##Vertice
#     def area_bar_page (self): 
#         self.page=Area_Bar_Popup_Page(self.altdriver)
        
#     def settings_page (self): 
#         self.page=Settings_Page(self.altdriver,self.driver)
#     def credits_page (self): 
#         self.credits_page=Credits_Page(self.altdriver,self.driver)
#     def login_popup_page (self): 
#         self.page=Login_Popup_Page(self.altdriver)

#     def register_popup_page (self): 
#         self.page=Regitration_Popup_Page(self.altdriver)

#     def forgot_password_page (self): 
#         self.page=Forgot_Password_Page(self.altdriver)

#         ##Edge
#     def press_login_button(self):
#         self.page.press_login_button()
#     def press_register_button (self): 
#         self.page.press_register_button()
#     def press_forgot_password_button (self): 
#         self.page.press_forgot_password_button()

#     def fill_and_send_forgot_password_form (self): 
#         self.page.forgot_password()

#     def fill_and_send_register_form (self): 
#         self.page.register()

#     def login_with_valid_account (self): 
#         self.page.login('secondTestAccount@testsonbitbar.com','password123')
        
#     def press_setting_button (self): 
#         self.page.press_setting_button()
#     def press_credits_button (self): 
#         self.page.press_credits_button()
#     def press_close_settings (self): 
#         self.page.press_close_button()
#     def press_close_credits (self): 
#         self.page.press_back_button()
# class MyDeckModel(CZBTests):
#     def setUpModel(self):
#         self.page=super(MyDeckModel,self)
#         self.setUp()
#     def my_decks_page (self): 
#         self.page=Deck_Selection_Page(self.altdriver)
#     def overlord_selection_page (self): 
#         self.page=Overlord_Selection_Page(self.altdriver)
#     def overlord_ability_popup_page (self): 
#         self.page=Overlord_Ability_Popup_Page(self.altdriver)
#     def rename_deck_popup_page (self): 
#         self.page=Deck_Rename_Tab_Page(self.altdriver)
#     def horde_editing_page (self): 
#         self.page=Horde_Editing_Page(self.altdriver,self.driver)

#     def create_new_deck (self): 
#         self.page.create_new_deck()
#     def press_overlord_selection_page_continue_button (self): 
#         self.page.press_continue()
#     def press_overlord_ability_popup_page_continue_button (self): 
#         self.page.press_continue()
#     def press_save_button (self): 
#         self.page.press_save_button()
#     def add_card_to_deck (self): 
#         self.page.add_cards_to_horde(1)
#     def press_save_deck_button (self): 
#         self.page.press_save()
#     def select_overlord (self): 
#         self.page.select_overlord
#     def press_back_button_from_overlord_selection_page (self): 
#         self.page.press_back()
#     def press_back_button_from_overlord_ability_page (self): 
#         self.page.press_back()
#     def press_back_button_from_horde_editing_page (self): 
#         self.page.press_back()
# class BattleModel(CZBTests):
#     def setUpModel(self):
#         self.page=super(BattleModel,self)
#         self.setUp()
#     ##Vertice
#     def main_menu_page (self): 
#         self.page=Main_Menu_Page(self.altdriver)
#     def battle_mode_selection_pop_up_page (self): 
#         self.page=Battle_Mode_Selection_Popup_Page(self.altdriver)
#     ##Edge
#     def press_select_game_mode (self): 
#         self.page.press_battle_mode_button()
#     def select_solo_mode (self): 
#         self.page.press_solo_mode_button()
#     def select_pvp_mode(self):
#         self.page.press_pvp_mode_button()
#     def start_game(self):
#         self.page.press_battle_button()
#     def start_tutorial(self):
#         self.page.press_tutorial_button()
    
