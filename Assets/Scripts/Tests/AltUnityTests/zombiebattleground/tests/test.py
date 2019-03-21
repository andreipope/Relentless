import xmlrunner
from appium import webdriver

from altunityrunner import AltrunUnityDriver, NotFoundException
from .pages.area_bar_popup_page import Area_Bar_Popup_Page
from .pages.base import CZBTests
from .pages.forgot_password_page import Forgot_Password_Page
from .pages.login_popup_page import Login_Popup_Page
from .pages.main_menu_page import Main_Menu_Page
from .pages.register_popup_page import Regitration_Popup_Page
from .pages.succes_forgot_page import Succes_Forgot_Page
from .pages.wait_page import Wait_Page
from .pages.side_menu_popup_page import Side_Menu_Popup_Page
from .pages.forgot_password_page import Forgot_Password_Page
from .pages.horde_editing_page import Horde_Editing_Page
from .pages.overlord_selection_page import Overlord_Selection_Page
from .pages.overlord_ability_popup_page import Overlord_Ability_Popup_Page
from .pages.deck_rename_tab_page import Deck_Rename_Tab_Page
from .pages.battle_mode_selection_page import Battle_Mode_Selection_Page

class TestModel(CZBTests):
    def setUpModel(self):
        self.page=super(TestModel,self)
        self.setUp()

    
    ##vertices
    def main_menu_page(self):
        print("======mainMenu")
        self.page=Main_Menu_Page(self.altdriver)
    def area_bar_popup_page(self):
        print("======areaBar")
        self.page=Area_Bar_Popup_Page(self.altdriver)
    def login_popup_page(self):
        print("======loginPop")
        self.page=Login_Popup_Page(self.altdriver)
    def side_menu_page(self):
        print("======SideMenu")
        self.page=Side_Menu_Popup_Page(self.altdriver)
    def tutorial_popup_page(self):
        pass
    def my_shop_menu_page(self):
        pass
    def my_decks_page(self):
        pass
    def my_cars_page(self):
        pass
    def overlord_selection_page(self):
        self.page=Overlord_Selection_Page(self.altdriver)
    def overlord_ability_popup_page(self):
        self.page=Overlord_Ability_Popup_Page(self.altdriver)
    def battle_mode_selection_page(self):
        pass
    def my_packs_page(self):
        pass
    def my_cards_page(self):
        pass
    def rename_deck_popup_page(self):
        pass
    def  horde_editing_page(self):
        self.page=Horde_Editing_Page(self.altdriver,self.driver)
    def forgot_password_page(self):
        self.page=Forgot_Password_Page(self.altdriver)
    def register_popup_page(self):
        self.page=Regitration_Popup_Page(self.altdriver)
    ##edges
    def login_with_valid_account(self):
        print("======loginAction")
        self.page.login('secondTestAccount@testsonbitbar.com','password123')
    def press_login_button(self):
        print("======pressLogin")
        self.page.press_login_button()
    def select_area_bar(self):
        print("==========selectArea")
    def press_no_button(self):
        print("==========pressNo")
        self.skip_tutorials()
    def press_register_button(self):
        self.page.press_register_button()
    def press_forgot_password_button(self):
        self.page.press_forgot_password_button()
    def fill_and_send_forgot_password_form(self):
        self.page.forgot_password()
    def fill_and_send_register_form(self):
        self.page.register()
    def select_side_menu(self):
        pass
    def press_my_decks_button(self):
        self.page.press_my_decks_button()
    def press_main_menu_button(self):
        self.page.press_battle_button()
    def press_my_cards_button(self):
        self.page_press_my_cards_button()
    def press_my_shop_button(self):
        self.page_press_my_shop_button()
    def press_open_packs_button(self):
        self.page.press_open_packs_button()
    def create_new_deck(self):
        self.page.create_new_deck()
    def press_overlord_selection_page_continue_button(self):
        self.page.press_continue()
    def press_battle_mode_button(self):
        pass
    def press_rename_button_from_horde_editing_page(self):
        pass
    def press_back_button_from_horde_editing_page(self):
        pass
    def press_back_button_from_overlord_ability_page(self):
        pass
    def press_back_button_from_overlord_selection_page(self):
        pass
    def select_overlord(self):
        pass
    def press_overlord_ability_popup_page_continue_button(self):
        self.page.press_continue_button()
    def press_save_button(self):
        self.page.press_save_button()
    def add_card_to_deck(self):
        self.page.add_cards_to_horde(1)
    def press_save_deck_button(self):
        self.page.press_save()
    def press_rename_button(self):
        pass
    def select_solo_mode(self):
        self.page.press_solo_mode()
    def select_pvp_mode(self):
        self.page.press_pvp_mode()
